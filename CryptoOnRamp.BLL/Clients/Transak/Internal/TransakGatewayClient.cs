using System.Net.Http.Json;
using CryptoOnRamp.BLL.Clients.Transak.Dto;
using CryptoOnRamp.BLL.Clients.Transak.Models;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Extensions;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.Transak.Internal;

internal sealed class TransakGatewayClient(HttpClient httpClient, IOptionsSnapshot<TransakClientOptions> optionsSnapshot) : ITransakGatewayClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TransakClientOptions _options = optionsSnapshot.Value;


    #region ITransakClient

    public async Task<WidgetResponse> BuildWidgetAsync(WidgetRequest request, CancellationToken cancellationToken)
    {
        var httpResponse = await _httpClient
            .PostAsJsonAsync(
                requestUri: "api/v2/auth/session",
                value:
                    new WidgetRequestDto
                    {
                        Parameters = new()
                        {
                            ApiEnvironment = _options.ApiEnvironment,
                            ApiKey = _options.ApiKey,
                            ReferrerDomain = _options.ReferrerDomain,
                            CryptoCurrencyCode = request.CryptoCurrency,
                            FiatCurrency = request.FiatCurrency,
                            FiatAmount = request.FiatAmount,
                            Network = request.Network,
                            WalletAddress = request.WalletAddress,
                            DisableWalletAddressForm = true,
                            RedirectUrl = _options.RedirectUrl,
                        },
                        LandingPage = _options.LandingPage,
                    },
                cancellationToken: cancellationToken);

        var result = await httpResponse.Content
            .ReadFromJsonAsync<ResponseDto<WidgetDto>>(cancellationToken);

        var widgetUrl = result?.Data?.Url ?? throw new AppException("Failed to get Transak widget url.", BusinessErrorCodes.NoData);

        return new WidgetResponse
        {
            WidgetUrl = widgetUrl,
            PartnerOrderId = request.PartnerOrderId,
            PartnerUserId = request.PartnerUserId,
        };
    }

    #endregion
}

