namespace CryptoOnRamp.BLL.Models;

public class TransactionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string FiatCurrency { get; set; } = string.Empty;
    public decimal FiatAmount { get; set; }
    public string? CryptoCurrency { get; set; }
    public decimal? CryptoAmount { get; set; }
    public string UserWallet { get; set; } = string.Empty;
    public string UniqueWalletAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? TxHash { get; set; }
    public FeeDto Fees { get; set; } = new();

    public List<PayoutDto> Payouts { get; set; } = new();

    public List<CheckoutSessionDTO> CheckoutSessions { get; set; } = [];
}

public class FeeDto
{
    public decimal SuperAgentPercent { get; set; }
    public decimal AgentPercent { get; set; }

    public decimal SuperAgent { get; set; }
    public decimal Agent { get; set; }


    public decimal NetPayout { get; set; }
}
