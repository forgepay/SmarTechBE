using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record WidgetRequestDto
{
    [JsonPropertyName("widgetParams")]
    public required WidgetParametersDto Parameters { get; set; }

    [JsonPropertyName("landingPage")]
    public required string LandingPage { get; set; }
}
