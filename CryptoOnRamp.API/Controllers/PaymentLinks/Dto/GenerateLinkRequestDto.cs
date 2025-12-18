using System.Text.Json.Serialization;

namespace CryptoOnRamp.API.Controllers.PaymentLinks.Dto;

public sealed class GenerateLinkRequestDto
{
    [JsonPropertyName("UserId")]
    public int? UserId { get; set; }

    [JsonPropertyName("FiatCurrency")]
    public string? FiatCurrency { get; set; }

    [JsonPropertyName("Amount")]
    public required decimal Amount { get; set; }
}
