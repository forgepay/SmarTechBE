using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Models;

public sealed record PayoutPartyRequestDto
{
    [JsonPropertyName("address")]
    public required string Address { get; init; }

    [JsonPropertyName("percentage")]
    public required decimal Percentage { get; init; }
}
