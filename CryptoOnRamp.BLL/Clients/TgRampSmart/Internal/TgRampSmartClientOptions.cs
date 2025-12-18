using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Internal;

internal sealed record TgRampSmartClientOptions
{
    public const string Position = "TgRampSmart";

    [JsonPropertyName("endpoint")]
    public required string Endpoint { get; init; }
}
