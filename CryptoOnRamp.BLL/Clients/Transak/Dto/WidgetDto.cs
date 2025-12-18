using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record WidgetDto
{
    [JsonPropertyName("widgetUrl")]
    public required string Url { get; set; }
}