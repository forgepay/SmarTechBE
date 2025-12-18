using System.Net.Http.Json;
using CryptoOnRamp.BLL.Clients.Transak.Enums;
using CryptoOnRamp.BLL.Clients.Transak.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Extensions;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.Transak.Internal;

internal sealed class TransakClient(HttpClient httpClient, IOptionsSnapshot<TransakClientOptions> optionsSnapshot) : ITransakClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TransakClientOptions _options = optionsSnapshot.Value;


    #region ITransakClient

    public async Task<CredentialsDto> GetAccessCredentialsAsync(CancellationToken cancellationToken)
    {
        var httpResponse = await _httpClient
            .PostAsJsonAsync(
                requestUri: "partners/api/v2/refresh-token",
                value:
                    new AccessTokenRequestDto
                    {
                        ApiKey = _options.ApiKey,
                    },
                options: options => options
                    .Set("AuthorizationType", AuthorizationTypes.ApiSecret),
                cancellationToken: cancellationToken);

        var result = await httpResponse.Content
            .ReadFromJsonAsync<ResponseDto<CredentialsDto>>(cancellationToken);

        return result?.Data ?? throw new AppException("Failed to get access token from Transak.", BusinessErrorCodes.NoData);
    }

    #endregion
}

