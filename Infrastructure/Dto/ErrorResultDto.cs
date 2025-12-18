using MicPic.Infrastructure.Exceptions;
using System.Collections;
using System.Text.Json.Serialization;

namespace MicPic.Infrastructure.Dto;

public sealed record ErrorResultDto
{

    [JsonPropertyName("error-code")]
    public BusinessErrorCodes ErrorCode { get; init; } = BusinessErrorCodes.GeneralError;

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("technical-message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TechnicalMessage { get; init; }

    [JsonPropertyName("technical-data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary? TechnicalData { get; init; }
}
