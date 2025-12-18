using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record ErrorDto
{
    [JsonPropertyName("statusCode")]
    public required int StatusCode { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

/*
{
  "error": {
    "statusCode": 401,
    "name": "Unauthorized",
    "message": "Authorization Required"
  }
}
*/