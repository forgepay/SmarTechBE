using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CryptoOnRamp.BLL.Services;

public sealed class PaybisWidgetLinkBuilder(IOptions<PaybisOptions> opt) : IPaybisWidgetLinkBuilder
{
    private readonly PaybisOptions _opt = opt.Value;

    public Task<Uri> GetWidgetUrlAsync(PaybisWidgetPetition petition)
    {
        var args =
            new List<KeyValuePair<string, object?>>
            {
            new("partnerId", _opt.PartnerId),
            new("partnerUserId", petition.PartnerUserId),
            new("cryptoAddress", petition.CryptoAddress),
            new("currencyCodeFrom", petition.CurrencyCodeFrom),
            new("currencyCodeTo", petition.CurrencyCodeTo),
            new("amountFrom", petition.AmountFrom),
            };

        var query = string.Concat('?', args.ToQueryString());

        var signatuire = HMACSHA256.HashData(
            key: Convert.FromBase64String(_opt.HmacKey),
            source: Encoding.UTF8.GetBytes(query));

        args.Add(new("signature", Convert.ToBase64String(signatuire)));

        var builder =
            new UriBuilder(_opt.Endpoint)
            {
                Query = args.ToQueryString(),
            };

        return Task.FromResult(builder.Uri);
    }


    public Uri BuildLink(PaybisWidgetPetition p)
    {
        // Минимально достаточный набор параметров под их "Web: Direct URL standalone integration"
        var args = new List<KeyValuePair<string, object?>>
    {
        new("partnerId",        _opt.PartnerId),
        new("partnerUserId",    p.PartnerUserId),                    // РЕКОМЕНДУЮ класть tx.Id.ToString()
        new("transactionFlow",  p.Flow.ToString()),                  // "BuyCrypto" | "SellCrypto"
        new("cryptoAddress",    p.CryptoAddress),
        new("currencyCodeFrom", p.CurrencyCodeFrom),                 // "EUR" | "USD"
        new("currencyCodeTo",   p.CurrencyCodeTo),                   // "USDC-POLYGON" | "USDC-ETHEREUM"
        new("amountFrom",       p.AmountFrom > 0 ? p.AmountFrom.ToString("0.##", CultureInfo.InvariantCulture) : null),
        new("amountTo",         p.AmountTo   > 0 ? p.AmountTo.ToString("0.########", CultureInfo.InvariantCulture) : null),
    };

        // 1) Строим query БЕЗ подписи
        var queryNoSig = "?" + ToQueryString(args);

        // 2) Подписываем ИМЕННО строку с ведущим '?'
        var key = Convert.FromBase64String(_opt.HmacKey);
        var signatureBytes = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(queryNoSig));
        var signatureB64 = Convert.ToBase64String(signatureBytes);

        // 3) Добавляем signature как параметр
        args.Add(new("signature", signatureB64));

        // 4) Итоговый URL
        var builder = new UriBuilder(_opt.Endpoint?.TrimEnd('/') ?? "https://widget.sandbox.paybis.com")
        {
            Query = ToQueryString(args) // без '?', UriBuilder сам добавит
        };
        return builder.Uri;
    }

    private static string ToQueryString(IEnumerable<KeyValuePair<string, object?>> args)
    {
        static string Enc(string s) => Uri.EscapeDataString(s);
        static string? Str(object? o) =>
            o switch
            {
                null => null,
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => o.ToString()
            };

        return string.Join("&",
            args.Where(kv => kv.Value is not null && !string.IsNullOrWhiteSpace(Str(kv.Value)))
                .Select(kv => $"{Enc(kv.Key)}={Enc(Str(kv.Value)!)}"));
    }
}
