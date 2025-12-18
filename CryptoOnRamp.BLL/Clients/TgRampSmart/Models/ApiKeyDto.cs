using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Models;

public class ApiKeyDto
{
    [JsonPropertyName("api-key")]
    public required string ApiKey { get; set; }
}