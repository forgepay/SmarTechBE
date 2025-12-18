namespace CryptoOnRamp.BLL.Services.ExchangeRate.Internal;

public interface IExchangeRateDataProvider
{
    Task<decimal?> GetExchangeRateOrDefaultAsync(string currency, string quote, CancellationToken cancellationToken);
}

