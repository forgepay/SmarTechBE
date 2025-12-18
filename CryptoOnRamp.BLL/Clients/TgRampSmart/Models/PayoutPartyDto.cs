using System.Numerics;
using System.Text.Json.Serialization;
using MicPic.Infrastructure.Serialization.Converters;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Models;

public sealed record PayoutPartyDto
{
    [JsonPropertyName("address")]
    public required string Address { get; init; }

    [JsonPropertyName("percentage")]
    public required decimal Percentage { get; init; }

    [JsonPropertyName("amount")]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger? Amount { get; init; }
}