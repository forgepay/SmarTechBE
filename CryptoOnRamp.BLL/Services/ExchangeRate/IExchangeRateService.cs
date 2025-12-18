namespace CryptoOnRamp.BLL.Services.ExchangeRate.Internal;

public interface IExchangeRateService
{
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string quote, CancellationToken cancellationToken);
}

