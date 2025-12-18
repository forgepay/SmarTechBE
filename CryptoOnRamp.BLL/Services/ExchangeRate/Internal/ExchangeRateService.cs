using CryptoOnRamp.BLL.Clients.ExchangeRateApi.Internal;

namespace CryptoOnRamp.BLL.Services.ExchangeRate.Internal;

internal sealed class ExchangeRateService(
    IExchangeRateApiClient exchangeRateApiClient) : IExchangeRateService
{
    private readonly IExchangeRateApiClient _exchangeRateApiClient = exchangeRateApiClient;


    #region IExchangeRateService

    public async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string quote, CancellationToken cancellationToken)
    {
        var response = await _exchangeRateApiClient
            .GetExchangeRatesAsync(quote, cancellationToken);

        return response.ConversionRates;
    }

    #endregion
}
