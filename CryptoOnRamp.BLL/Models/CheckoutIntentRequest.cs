using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

public sealed class CheckoutIntentRequest
{
    [JsonPropertyName("onramp")]
    public string Onramp { get; set; } = "";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = "";

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "buy";

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = "";

    [JsonPropertyName("network")]
    public string Network { get; set; } = "";

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("originatingHost")]
    public string OriginatingHost { get; set; } = "buy.onramper.com";

    [JsonPropertyName("partnerContext")]
    public string PartnerContext { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("wallet")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Wallet? Wallet { get; set; }

    [JsonPropertyName("signature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Signature { get; set; }

    [JsonPropertyName("signContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SignContent { get; set; }

    [JsonPropertyName("supportedParams")]
    public SupportedParams SupportedParams { get; set; } = new();
}

public sealed class SupportedParams
{
    [JsonPropertyName("partnerData")]
    public PartnerData PartnerData { get; set; } = new();
}

public sealed class PartnerData
{
    [JsonPropertyName("redirectUrl")]
    public RedirectUrl RedirectUrl { get; set; } = new();
}

public sealed class RedirectUrl
{
    [JsonPropertyName("success")]
    public string Success { get; set; } = Uri.EscapeDataString("https://yourapp.com/onramp/success");
}
