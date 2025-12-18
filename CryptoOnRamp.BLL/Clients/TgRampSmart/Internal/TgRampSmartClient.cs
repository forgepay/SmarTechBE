using System.Net.Http.Json;
using CryptoOnRamp.BLL.Clients.TgRampSmart.Models;
using MicPic.Infrastructure.Dto;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Internal;

internal sealed class TgRampSmartClient(HttpClient httpClient) : ITgRampSmartClient
{
    private readonly HttpClient _httpClient = httpClient;


    #region ITgRampSmartClient

    public async Task<ApiKeyDto> GenerateApiKeyAsync(CancellationToken cancellationToken)
    {
        var httpResponse = await _httpClient
            .PostAsync("api/v1/apikeys", default, cancellationToken);

        var result = await httpResponse.Content
            .ReadFromJsonAsync<ServiceResponseDto<ApiKeyDto>>(cancellationToken);

        return result?.Data ?? throw new ApplicationException("Failed to create ApiKey");
    }

    public async Task<PayoutDto> CreatePayoutAsync(string apiKey, PayoutRequestDto request, CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/payouts");
        
        httpRequest.Headers.Add("X-COR-ApiKey", apiKey);
        httpRequest.Content = JsonContent.Create(request);
        
        var httpResponse = await _httpClient
            .SendAsync(httpRequest, cancellationToken);

        var result = await httpResponse.Content
            .ReadFromJsonAsync<ServiceResponseDto<PayoutDto>>(cancellationToken);

        return result?.Data ?? throw new ApplicationException("Failed to create payout");
    }

    public async Task<PayoutDto> ClaimPayoutAsync(string apiKey, Guid id, CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/v1/payouts/{id}/claim");

        httpRequest.Headers.Add("X-COR-ApiKey", apiKey);
        
        var httpResponse = await _httpClient
            .SendAsync(httpRequest, cancellationToken);

        var result = await httpResponse.Content
            .ReadFromJsonAsync<ServiceResponseDto<PayoutDto>>(cancellationToken);

        return result?.Data ?? throw new ApplicationException("Failed to claim payout");
    }

    #endregion
}
