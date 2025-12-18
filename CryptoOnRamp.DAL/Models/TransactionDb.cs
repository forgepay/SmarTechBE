namespace CryptoOnRamp.DAL.Models;

public enum TransactionStatusDb { Issued, Pending, Completed, PayoutCompleted, Failed, Expired }

public enum PayoutStatusDb { Pending, Completed, Failed, }

public class TransactionDb
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserDb User { get; set; } = null!;

    public string Provider { get; set; } = string.Empty;
    public string? ExternalId { get; set; }

    public string FiatCurrency { get; set; } = string.Empty;
    public decimal FiatAmount { get; set; }
    public string? CryptoCurrency { get; set; } = "USDC";
    public decimal? CryptoAmount { get; set; }
    public string UserWallet { get; set; } = string.Empty;
    public string UniqueWalletAddress { get; set; } = string.Empty;
    public string UniquePrivateKey { get; set; } = string.Empty;

    public TransactionStatusDb Status { get; set; } = TransactionStatusDb.Issued;
    public DateTime Timestamp { get; set; }

    public string? TxHash { get; set; }                 // nullable before payout


    public decimal SuperAgentPercent { get; set; }
    public decimal AgentPercent { get; set; }

    public decimal SuperAgentFee { get; set; }
    public decimal AgentFee { get; set; }
    public decimal NetPayout { get; set; }

    public List<PayoutDb> Payouts { get; set; } = [];

    public List<CheckoutSessionDb> CheckoutSessions { get; set; } = [];

    public string? ContractApiKey { get; set; }
    public Guid? ContractUniqueId { get; set; }
}

public class PayoutDb
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public TransactionDb Transaction { get; set; } = null!;
    public PayoutType Type { get; set; }

    public string ToWallet { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;

    public DateTime CreatetAt { get; set; }
    public string CreatetBy { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
    public PayoutStatusDb Status { get; set; }
}
