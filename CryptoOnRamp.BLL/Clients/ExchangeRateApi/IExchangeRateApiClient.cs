using CryptoOnRamp.BLL.Clients.ExchangeRateApi.Models;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi.Internal;

public interface IExchangeRateApiClient
{
    Task<ResponseDto> GetExchangeRatesAsync(string quote, CancellationToken cancellationToken);
}
