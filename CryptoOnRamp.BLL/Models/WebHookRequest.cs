using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

internal record WebHookRequest<TPayload>(string Action, TPayload Payload)
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = Action;

    [JsonPropertyName("payload")]
    public TPayload Payload { get; set; } = Payload;
};
