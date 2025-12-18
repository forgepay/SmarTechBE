namespace CryptoOnRamp.BLL.Services.LinkGenerator.Models;

public sealed record LinkRequest
{
    public required int UserId { get; init; }

    public required int TransactionId { get; init; }

    public required string CryptoCurrency { get; init; }

    public required string FiatCurrency { get; init; }

    public required decimal FiatAmount { get; init; }

    public required string WalletAddress { get; init; }
}
