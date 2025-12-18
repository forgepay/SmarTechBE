namespace CryptoOnRamp.BLL.Interfaces;

public interface ILinkGenerationOrchestrator
{
    Task<GenerateLinkResult> GenerateAsync(LinkGenerationInput input, CancellationToken ct);
}

public sealed class LinkGenerationInput
{
    public int? RequestorUserId { get; init; }
    public int TargetUserId { get; init; }
    public string FiatCurrency { get; init; } = "EUR";
    public decimal Amount { get; init; }
    public string? Email { get; init; }
}

public sealed class GenerateLinkResult
{
    public int TransactionId { get; init; }
    public string UniqueWalletAddress { get; init; } = "";
    public string EncryptedPrivateKey { get; init; } = "";
    public List<string> PaymentLinks { get; init; } = new();
}
