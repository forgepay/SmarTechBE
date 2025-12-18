using CryptoOnRamp.BLL.Clients.Transak.Models;

namespace CryptoOnRamp.BLL.Clients.Transak;

public interface ITransakGatewayClient
{
    Task<WidgetResponse> BuildWidgetAsync(WidgetRequest request, CancellationToken cancellationToken);
}

