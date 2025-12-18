using CryptoOnRamp.BLL.Clients.ExchangeRateApi.Models;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Serialization;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi.HttpHandlers;

internal sealed class ErrorHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpResponseMessage = await base.SendAsync(request, cancellationToken);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            var response = AppJsonSerializer.DeserializeOrDefault<ResponseErrorDto>(content);

            if (!string.IsNullOrWhiteSpace(response?.ErrorType))
                throw new AppException(response.ErrorType, BusinessErrorCodes.Internal3rdPartyError);

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        return httpResponseMessage;
    }
}
