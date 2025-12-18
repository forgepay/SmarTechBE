using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

public sealed class PaybisWebhookPayload
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("event")]
    public string Event { get; set; } = "";

    [JsonPropertyName("data")]
    public PaybisWebhookData Data { get; set; } = new();
}

public sealed class PaybisWebhookData
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = "";

    [JsonPropertyName("partnerUserId")]
    public string PartnerUserId { get; set; } = "";

    [JsonPropertyName("transaction")]
    public PaybisWebhookTransaction Transaction { get; set; } = new();

    [JsonPropertyName("amountTo")]
    public PaybisAmountInfo AmountInfo { get; set; } = new();
}

public class PaybisAmountInfo
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "";
}

public sealed class PaybisWebhookTransaction
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}
