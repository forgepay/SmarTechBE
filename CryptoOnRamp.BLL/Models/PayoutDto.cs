using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL.Models;

public class PayoutDto
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public PayoutType Type { get; set; }

    public string ToWallet { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;

    public DateTime CreatetAt { get; set; }
    public string CreatetBy { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
    public PayoutStatusDb Status { get; set; }
}
