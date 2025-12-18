namespace CryptoOnRamp.BLL.Clients.Transak.Models;

public sealed record WidgetResponse
{
    public required string WidgetUrl { get; init; }

    public required string PartnerUserId { get; init; }

    public required string PartnerOrderId { get; init; }

}