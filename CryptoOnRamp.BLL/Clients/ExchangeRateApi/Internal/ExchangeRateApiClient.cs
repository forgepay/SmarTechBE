using System.Net.Http.Json;
using CryptoOnRamp.BLL.Clients.ExchangeRateApi.Models;
using MicPic.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi.Internal;

internal sealed class ExchangeRateApiClient(HttpClient httpClient, IOptionsSnapshot<ExchangeRateApiClientOptions> optionsSnapshot) : IExchangeRateApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ExchangeRateApiClientOptions _options = optionsSnapshot.Value;


    #region IExchangeRateApiClient

    public async Task<ResponseDto> GetExchangeRatesAsync(string quote, CancellationToken cancellationToken)
    {
        var apiKey = _options.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new AppException("ExchangeRateApi ApiKey is not set", BusinessErrorCodes.Misconfigured);
        }

        var httpResponse = await _httpClient
            .GetAsync($"v6/{apiKey}/latest/{quote}", cancellationToken);

        var result = await httpResponse.Content
            .ReadFromJsonAsync<ResponseDto>(cancellationToken);

        return result ?? throw new AppException(BusinessErrorCodes.NoData);
    }

    #endregion
}
