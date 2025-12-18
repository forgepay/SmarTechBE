namespace CryptoOnRamp.BLL.Models;

public sealed class GenerateLinkRequest
{
    public int? UserId { get; set; }
    public string FiatCurrency { get; set; } = "USD";
    public decimal Amount { get; set; }
}

public sealed class GenerateLinkResponse
{
    public int TransactionId { get; init; }
    public List<string> PaymentLinks { get; init; } = new();
    public string UniqueWalletAddress { get; init; } = "";
}
