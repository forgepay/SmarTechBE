using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record ResponseDto<T>
{
    [JsonPropertyName("data")]
    public required T Data { get; set; }
}
