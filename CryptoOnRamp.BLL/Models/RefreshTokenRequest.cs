using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

public class RefreshTokenRequest
{
    [JsonPropertyName("token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
