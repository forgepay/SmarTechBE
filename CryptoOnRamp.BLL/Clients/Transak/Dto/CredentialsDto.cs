using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record CredentialsDto
{
    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("expiresAt")]
    public required long ExpiresAt { get; set; }
}
