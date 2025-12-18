using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Models;

public sealed record PayoutDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("network")]
    public required string Network { get; set; }

    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("tx_hash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TxHash { get; set; }

    [JsonPropertyName("parties")]
    public Collection<PayoutPartyDto> Parties { get; init; } = [];    
}
