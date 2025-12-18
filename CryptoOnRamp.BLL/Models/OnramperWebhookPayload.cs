using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

public sealed class OnramperWebhookPayload
{
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = "";

    [JsonPropertyName("country")]
    public string Country { get; set; } = "";

    [JsonPropertyName("inAmount")]
    public decimal InAmount { get; set; }

    [JsonPropertyName("onramp")]
    public string Onramp { get; set; } = "";

    [JsonPropertyName("onrampTransactionId")]
    public string OnrampTransactionId { get; set; } = "";

    [JsonPropertyName("outAmount")]
    public decimal? OutAmount { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("partnerContext")]
    public string? PartnerContext { get; set; }

    [JsonPropertyName("sourceCurrency")]
    public string SourceCurrency { get; set; } = "";

    [JsonPropertyName("status")]
    public OnramperTransactionStatus Status { get; set; }

    [JsonPropertyName("statusDate")]
    public DateTime StatusDate { get; set; }

    [JsonPropertyName("targetCurrency")]
    public string TargetCurrency { get; set; } = "";

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = "";

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; set; } = "";

    [JsonPropertyName("transactionHash")]
    public string? TransactionHash { get; set; }

    [JsonPropertyName("walletAddress")]
    public string? WalletAddress { get; set; }

    [JsonPropertyName("partnerFee")]
    public decimal? PartnerFee { get; set; }
}

public class OnramperWebhookOptions
{
    public const string SectionName = "OnramperWebhook";

    [JsonPropertyName("secret")]
    public string Secret { get; set; } = "";        // HMAC секрет

    [JsonPropertyName("allowedApiKey")]
    public string AllowedApiKey { get; set; } = ""; // pk_... (опц. доп.проверка)
}
