using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi.Models;

public sealed record ResponseDto
{
    [JsonPropertyName("result")]
    public required string Result { get; set; }

    [JsonPropertyName("documentation")]
    public required string Documentation { get; set; }

    [JsonPropertyName("terms_of_use")]
    public required string TermsOfUse { get; set; }

    [JsonPropertyName("time_last_update_unix")]
    public required long TimeLastUpdateUnix { get; set; }

    [JsonPropertyName("time_last_update_utc")]
    public required string TimeLastUpdateUtc { get; set; }

    [JsonPropertyName("time_next_update_unix")]
    public required long TimeNextUpdateUnix { get; set; }

    [JsonPropertyName("time_next_update_utc")]
    public required string TimeNextUpdateUtc { get; set; }
    
    [JsonPropertyName("base_code")]
    public required string BaseCode { get; set; }

    [JsonPropertyName("conversion_rates")]
    public required Dictionary<string, decimal> ConversionRates { get; set; } = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
}
