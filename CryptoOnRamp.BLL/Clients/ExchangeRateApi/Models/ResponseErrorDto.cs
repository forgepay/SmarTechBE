using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi.Models;

public sealed record ResponseErrorDto
{
    [JsonPropertyName("result")]
    public required string Result { get; set; }

    [JsonPropertyName("error-type")]
    public string? ErrorType { get; set; }
}
