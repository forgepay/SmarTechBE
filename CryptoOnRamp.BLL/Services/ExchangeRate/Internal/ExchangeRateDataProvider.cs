using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Services.ExchangeRate.Internal;

internal sealed class ExchangeRateDataProvider(
    IExchangeRateService exchangeRateService,
    IMemoryCache memoryCache,
    IOptionsSnapshot<ExchangeRateServiceOptions> optionsSnapshot) : IExchangeRateDataProvider
{
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ExchangeRateServiceOptions _options = optionsSnapshot.Value;


    #region IExchangeRateDataProvider

    public async Task<decimal?> GetExchangeRateOrDefaultAsync(string currency, string quote, CancellationToken cancellationToken)
    {
        var rates = await GetExchangeRatesAsync(currency, cancellationToken);

        return rates.TryGetValue(quote, out var rate) ? rate : null;
    }

    #endregion


    #region Private Members

    private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string currency, CancellationToken cancellationToken)
    {
        var result = await _memoryCache.GetOrCreateAsync(
            key: $"{GetType().FullName}_{currency}",
            factory: async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _options.CacheExpiry;
                
                return await _exchangeRateService
                    .GetExchangeRatesAsync(currency, cancellationToken);
            });

        return result ?? [];
    }

    #endregion
}
