using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

public class AuthenticationContext
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("accessTokenExpiredAt")]
    public DateTime AccessTokenExpiredAt { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("refreshTokenExpiredAt")]
    public DateTime RefreshTokenExpiredAt { get; set; }
}
