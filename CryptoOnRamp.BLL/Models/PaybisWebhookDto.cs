namespace CryptoOnRamp.BLL.Models;

public sealed class PaybisWebhookDto
{
    public string RequestId { get; set; } = "";
    public string Status { get; set; } = ""; // "PENDING" | "COMPLETED" | "FAILED"      
}
