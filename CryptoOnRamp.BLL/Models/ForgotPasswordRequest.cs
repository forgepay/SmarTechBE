using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

public class ForgotPasswordRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
