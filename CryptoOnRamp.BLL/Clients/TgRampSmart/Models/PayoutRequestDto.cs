using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Models;

public sealed record PayoutRequestDto
{
    [JsonPropertyName("network")]
    public string Network { get; init; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; init; } = "USDT";

    [JsonPropertyName("parties")]
    public Collection<PayoutPartyRequestDto> Parties { get; init; } = [];
}
