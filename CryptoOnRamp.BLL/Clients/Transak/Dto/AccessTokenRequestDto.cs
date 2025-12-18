using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record AccessTokenRequestDto
{
    [JsonPropertyName("apiKey")]
    public required string ApiKey { get; set; }
}
