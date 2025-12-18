using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL.Models;

public class CheckoutSessionDTO
{
    public required string Id { get; set; }
    public int TransactionId { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string Url { get; set; } = "";
    public TransactionStatusDb Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpireAt { get { return this.CreatedAt.AddMinutes(25); } }
}
