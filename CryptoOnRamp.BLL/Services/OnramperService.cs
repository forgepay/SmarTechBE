using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace CryptoOnRamp.BLL.Services;

public sealed class OnramperService : IOnramperService
{
    private readonly HttpClient _http;
    private readonly AppilcationSettings _applicationSettings;
    private readonly OnramperOptions _options;
    private readonly ILogger _logger;

    public OnramperService(HttpClient http, IOptions<AppilcationSettings> applicationSettings, IOptions<OnramperOptions> options, ILogger<OnramperService> logger)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _applicationSettings = applicationSettings.Value;
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.ApiKey)) throw new ArgumentException("Onramper ApiKey must be set.");
        if (string.IsNullOrWhiteSpace(_options.BaseUrl)) throw new ArgumentException("Onramper BaseUrl must be set.");

        _http.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);

        // ✅ Correct Authorization header
        _http.DefaultRequestHeaders.Remove("Authorization");
        _http.DefaultRequestHeaders.Add("Authorization", _options.ApiKey);

        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _logger = logger;
    }

    public async Task<List<QuoteItem>> GetBuyQuotesAsync(
        string fiat, string crypto, decimal amount,
        string? country = null, string? network = null, string? paymentMethod = null,
        CancellationToken ct = default)
    {
        var query = new List<string> { $"amount={amount}" };
        if (!string.IsNullOrWhiteSpace(country)) query.Add($"country={Uri.EscapeDataString(country)}");
        if (!string.IsNullOrWhiteSpace(network)) query.Add($"network={Uri.EscapeDataString(network)}");
        if (!string.IsNullOrWhiteSpace(paymentMethod)) query.Add($"paymentMethod={Uri.EscapeDataString(paymentMethod)}");

        var path = $"quotes/{fiat}/{crypto}";
        var url = query.Count > 0 ? $"{path}?{string.Join("&", query)}" : path;

        using var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var str = await resp.Content.ReadAsStringAsync();

        return (await resp.Content.ReadFromJsonAsync<List<QuoteItem>>(cancellationToken: ct))
               ?? new List<QuoteItem>();
    }

    private static List<QuoteItem> PickRoute(IEnumerable<QuoteItem> quotes, string preferredMethod = "creditcard", decimal? amount = null)
    {
        var ok = quotes
        .Where(q => q.Errors == null || q.Errors.Count == 0)
        .ToList();

        // Must support preferred payment method (either chosen or available)
        ok = ok.Where(q =>
               string.Equals(q.PaymentMethod, preferredMethod, StringComparison.OrdinalIgnoreCase) ||
               (q.AvailablePaymentMethods?.Any(pm => pm.PaymentTypeId.Equals(preferredMethod, StringComparison.OrdinalIgnoreCase)) ?? false)
            ).ToList();

        // Respect min/max if present
        if (amount.HasValue)
        {
            ok = ok.Where(q =>
            {
                var pm = q.AvailablePaymentMethods?.FirstOrDefault(pm => pm.PaymentTypeId.Equals(preferredMethod, StringComparison.OrdinalIgnoreCase));
                var min = pm?.Details?.Limits?.AggregatedLimit?.Min ?? 0m;
                var max = pm?.Details?.Limits?.AggregatedLimit?.Max ?? decimal.MaxValue;
                return amount.Value >= min && amount.Value <= max;
            }).ToList();
        }

        // Prefer recommendations
        var recommended = ok.Where(q => q.Recommendations?.Any(r => r is "BestPrice" or "Recommended") == true).ToList();
        if (recommended.Any()) ok = recommended;

        // Highest payout (or lowest total fee) wins
        return ok
            .OrderByDescending(q => q.Payout ?? 0m)
            .ThenBy(q => (q.NetworkFee ?? 0m) + (q.TransactionFee ?? 0m)).ToList();
    }

    public async Task<List<OnramperCheckoutResponse>> CreateBestCheckoutIntentsAsync(
        string fiat, string crypto, decimal amount,
        string? walletAddress, string? walletMemo,
        int transactionId,                       // ← важно: это id транзакции
        CancellationToken ct = default)
    {
        // 1) тянем quotes
        var quotes = await GetBuyQuotesAsync(fiat, crypto, amount, country: null, network: "polygon", paymentMethod: null, ct);
        
        // filter
        quotes = quotes
            .Where(q => !_options.DisabledProviders.Contains(q.Ramp))
            .ToList();
        
        var routes = PickRoutes(quotes).ToList();

        if (routes.Count == 0)
            throw new InvalidOperationException("No available onramp route for the selected fiat/crypto/amount.");

        var (signContent, signature) = BuildSignature(walletAddress, walletMemo);

        var results = new List<OnramperCheckoutResponse>();

        // 2) создаём intents по каждому маршруту
        for (int i = 0; i < routes.Count && results.Count <= 20; i++)
        {
            try
            {
                var r = routes[i];
                var req = new CheckoutIntentRequest
                {
                    Onramp = r.Ramp,
                    Source = fiat.ToLowerInvariant(),
                    Destination = crypto.ToLowerInvariant(),
                    Amount = amount,
                    Type = "buy",
                    PaymentMethod = r.PaymentMethod,
                    Network = "polygon",
                    PartnerContext = $"{transactionId}:{i + 1}",
                    //Email = email,
                    SupportedParams = new SupportedParams() { PartnerData = new PartnerData() { RedirectUrl = new RedirectUrl() { Success = $"{_applicationSettings.UrlUi.TrimEnd('/')}/api/Webhook/onramper" } } }
                };

                if (!string.IsNullOrEmpty(walletAddress))
                {
                    req.Wallet = new Wallet { Address = walletAddress, Memo = walletMemo };
                    if (!string.IsNullOrEmpty(_options.SignSecret))
                    {
                        req.Signature = signature;
                        req.SignContent = signContent;
                    }
                }

                using var resp = await _http.PostAsJsonAsync("checkout/intent", req, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"Onramper /checkout/intent {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");

                var intent = (await resp.Content.ReadFromJsonAsync<OnramperCheckoutResponse>(cancellationToken: ct))!;
                results.Add(intent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create intent.");
            }
           
        }

        return results;
    }

    // Выбираем маршруты без ошибок, дедуп по (ramp,paymentMethod), сортируем по рекомендациям и payout
    private static IEnumerable<(string Ramp, string PaymentMethod)> PickRoutes(List<QuoteItem> quotes)
    {
        var items = quotes
            .Where(q => q.Errors == null || q.Errors.Count == 0)
            .Where(q => !string.IsNullOrWhiteSpace(q.Ramp) && !string.IsNullOrWhiteSpace(q.PaymentMethod))
            .ToList();

        // приоритет BestPrice/Recommended → больший payout
        var ordered = items
            .OrderByDescending(q => q.Recommendations?.Any(r => r is "BestPrice" or "Recommended") == true)
            .ThenByDescending(q => q.Payout ?? 0m).ToList();

        // dedup по (ramp, paymentMethod)
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var q in ordered)
        {
            var key = $"{q.Ramp}|{q.PaymentMethod}";
            if (seen.Add(key))
                yield return (q.Ramp, q.PaymentMethod);
        }

        //foreach (var q in ordered)
        //{
        //    foreach(var qq in q.AvailablePaymentMethods)
        //    {
        //        var key = $"{q.Ramp}|{qq.PaymentTypeId}";
        //        if (seen.Add(key))
        //            yield return (q.Ramp, qq.PaymentTypeId);
        //    }
        //}
    }

    public async Task<OnramperCheckoutResponse> CreateCheckoutIntentAsync(
        string onramp, string fiat, string crypto, decimal amount,
        string type, string paymentMethod, string network,
        string? walletAddress, string? walletMemo,
        string partnerContext, CancellationToken ct = default)
    {
        // нормализуем вход
        walletAddress = string.IsNullOrWhiteSpace(walletAddress) ? null : walletAddress.Trim();
        walletMemo = string.IsNullOrWhiteSpace(walletMemo) ? null : walletMemo.Trim();

        var (signContent, signature) = BuildSignature(walletAddress, walletMemo);

        var req = new CheckoutIntentRequest
        {
            Onramp = onramp,
            Source = fiat.ToLowerInvariant(),
            Destination = crypto.ToLowerInvariant(),
            Amount = amount,
            Type = type,
            PaymentMethod = paymentMethod,
            Network = network,
            PartnerContext = partnerContext,
            //Email = email
        };

        // кошелёк отправляем только если есть адрес
        if (!string.IsNullOrEmpty(walletAddress))
            req.Wallet = new Wallet { Address = walletAddress, Memo = walletMemo };

        // подпись отправляем, если присутствует address ИЛИ memo, и есть секрет
        if ((!string.IsNullOrEmpty(walletAddress) || !string.IsNullOrEmpty(walletMemo)) &&
            !string.IsNullOrEmpty(_options.SignSecret))
        {
            req.SignContent = signContent;
            req.Signature = signature;
        }

        using var resp = await _http.PostAsJsonAsync("checkout/intent", req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"Onramper /checkout/intent {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");

        return (await resp.Content.ReadFromJsonAsync<OnramperCheckoutResponse>(cancellationToken: ct))!;
    }

    public (string signContent, string signature) BuildSignature(string? walletAddress, string? walletMemo)
    {
        if (string.IsNullOrEmpty(_options.SignSecret))
            return (string.Empty, string.Empty);

        // Собираем пары и сортируем именно по ключам
        var dict = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(walletAddress)) dict["walletAddress"] = walletAddress!;
        if (!string.IsNullOrEmpty(walletMemo)) dict["walletMemo"] = walletMemo!;

        if (dict.Count == 0) return (string.Empty, string.Empty);

        var signContent = string.Join("&", dict.Select(kv => $"{kv.Key}={kv.Value}"));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SignSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signContent));
        var signature = Convert.ToHexString(hash).ToLowerInvariant();

        return (signContent, signature);
    }
}
