namespace CryptoOnRamp.BLL.Clients.Transak.Models;

public sealed record WidgetRequest
{
    public required string CryptoCurrency { get; init; }

    public required string FiatCurrency { get; init; }

    public required decimal FiatAmount { get; init; }

    public required string Network { get; init; }

    public required string WalletAddress { get; init; }

    public required string PartnerUserId { get; init; }

    public required string PartnerOrderId { get; init; }
}
