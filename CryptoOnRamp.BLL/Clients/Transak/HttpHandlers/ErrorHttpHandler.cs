using CryptoOnRamp.BLL.Clients.Transak.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.HttpHandlers;

internal sealed class ErrorHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpResponseMessage = await base.SendAsync(request, cancellationToken);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            var response = AppJsonSerializer.DeserializeOrDefault<ResponseErrorDto>(content);
            if (response?.Error is ErrorDto error)
            {
                if (!string.IsNullOrWhiteSpace(error.Message))
                    throw new AppException(error.Message, BusinessErrorCodes.Internal3rdPartyError);
            }

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        return httpResponseMessage;
    }
}
