namespace CryptoOnRamp.DAL.Models;

public class CheckoutSessionDb
{
    public required string Id { get; set; }
    public int TransactionId { get; set; }
    public TransactionDb Transaction { get; set; } = null!;
    public string Ramp { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public string? ExternalId { get; set; }         // провайдерский id (из вебхука)
    public string PartnerContext { get; set; } = ""; // наш контекст, уникальный на сессию
    public string Url { get; set; } = "";
    public TransactionStatusDb Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
