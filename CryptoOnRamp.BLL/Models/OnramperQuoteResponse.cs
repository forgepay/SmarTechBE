namespace CryptoOnRamp.BLL.Models;

using System.Text.Json.Serialization;

public sealed class QuoteItem
{
    [JsonPropertyName("rate")]
    public decimal? Rate { get; set; }

    [JsonPropertyName("networkFee")]
    public decimal? NetworkFee { get; set; }

    [JsonPropertyName("transactionFee")]
    public decimal? TransactionFee { get; set; }

    [JsonPropertyName("payout")]
    public decimal? Payout { get; set; }

    [JsonPropertyName("ramp")]
    public string Ramp { get; set; } = "";

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = "";

    [JsonPropertyName("quoteId")]
    public string QuoteId { get; set; } = "";

    [JsonPropertyName("recommendations")]
    public List<string>? Recommendations { get; set; }

    [JsonPropertyName("availablePaymentMethods")]
    public List<PaymentMethodInfo>? AvailablePaymentMethods { get; set; }

    [JsonPropertyName("errors")]
    public List<QuoteError>? Errors { get; set; }

    [JsonPropertyName("kycRequirements")]
    public KycRequirements? KycRequirements { get; set; }
}

public sealed class PaymentMethodInfo
{
    [JsonPropertyName("paymentTypeId")]
    public string PaymentTypeId { get; set; } = "";   // "creditcard", "sepainstant", ...

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "";

    [JsonPropertyName("details")]
    public PaymentDetails? Details { get; set; }
}

public sealed class PaymentDetails
{
    [JsonPropertyName("limits")]
    public Limits? Limits { get; set; }
}

public sealed class Limits
{
    [JsonPropertyName("aggregatedLimit")]
    public LimitBucket? AggregatedLimit { get; set; }

    // some payloads also include per-ramp limits like { "moonpay": { min, max } }
}

public sealed class LimitBucket
{
    [JsonPropertyName("min")]
    public decimal Min { get; set; }

    [JsonPropertyName("max")]
    public decimal Max { get; set; }
}

public sealed class QuoteError
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

public sealed class KycRequirements
{
    [JsonPropertyName("onramperLoginRequired")]
    public bool OnramperLoginRequired { get; set; }

    [JsonPropertyName("onramperKycRequirement")]
    public string OnramperKycRequirement { get; set; } = "";

    [JsonPropertyName("partnerLoginRequired")]
    public bool PartnerLoginRequired { get; set; }

    [JsonPropertyName("partnerKycRequirement")]
    public string PartnerKycRequirement { get; set; } = "";
}


public sealed class QuotesResponse
{
    [JsonPropertyName("quotes")] public List<QuoteItem> Quotes { get; set; } = new();
}

public sealed class OnramperCheckoutResponse
{
    [JsonPropertyName("message")] public CheckoutMessage Message { get; set; } = new();
}

public sealed class CheckoutMessage
{
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("sessionInformation")] public SessionInformation SessionInformation { get; set; } = new();
    [JsonPropertyName("transactionInformation")] public TransactionInformation TransactionInformation { get; set; } = new();
}

public sealed class SessionInformation
{
    [JsonPropertyName("sessionId")] public string SessionId { get; set; } = "";
    [JsonPropertyName("uuid")] public string Uuid { get; set; } = "";
    [JsonPropertyName("onramp")] public string Onramp { get; set; } = "";
    [JsonPropertyName("source")] public string Source { get; set; } = "";
    [JsonPropertyName("destination")] public string Destination { get; set; } = "";
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("paymentMethod")] public string PaymentMethod { get; set; } = "";
    [JsonPropertyName("network")] public string Network { get; set; } = "";
    [JsonPropertyName("wallet")] public Wallet Wallet { get; set; } = new();
}

public sealed class Wallet
{
    [JsonPropertyName("address")] public string Address { get; set; } = "";
    [JsonPropertyName("memo")] public string? Memo { get; set; }
}

public sealed class TransactionInformation
{
    [JsonPropertyName("transactionId")] public string TransactionId { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = ""; // iframe | redirect
}
