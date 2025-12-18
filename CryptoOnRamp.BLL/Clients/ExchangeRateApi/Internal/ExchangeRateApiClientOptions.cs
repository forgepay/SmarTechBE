using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi.Internal;

internal sealed record ExchangeRateApiClientOptions
{
    public const string Position = "ExchangeRateApi";

    [JsonPropertyName("endpoint")]
    public required string Endpoint { get; init; } = "https://v6.exchangerate-api.com";

    [JsonPropertyName("apiKey")]
    public required string ApiKey { get; init; } = string.Empty;
}
