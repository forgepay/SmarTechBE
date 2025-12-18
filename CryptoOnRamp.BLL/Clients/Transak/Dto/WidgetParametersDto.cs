using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record WidgetParametersDto
{
    [JsonPropertyName("environment")]
    public required string ApiEnvironment { get; set; }

    [JsonPropertyName("apiKey")]
    public required string ApiKey { get; set; }

    [JsonPropertyName("referrerDomain")]
    public required string ReferrerDomain { get; set; }

    [JsonPropertyName("cryptoCurrencyCode")]
    public required string CryptoCurrencyCode { get; set; }

    [JsonPropertyName("fiatCurrency")]
    public required string FiatCurrency { get; set; }

    [JsonPropertyName("fiatAmount")]
    public required decimal FiatAmount { get; set; }

    [JsonPropertyName("network")]
    public required string Network { get; set; }

    [JsonPropertyName("walletAddress")]
    public required string WalletAddress { get; set; }

    [JsonPropertyName("disableWalletAddressForm")]
    public bool DisableWalletAddressForm { get; set; } = true;

    [JsonPropertyName("redirectURL")]
    public required string RedirectUrl { get; set; }

    [JsonPropertyName("partnerOrderId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PartnerOrderId { get; set; }

    [JsonPropertyName("partnerCustomerId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PartnerCustomerId { get; set; }
}
