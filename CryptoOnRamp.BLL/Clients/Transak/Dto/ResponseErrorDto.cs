using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Clients.Transak.Dto;

internal sealed record ResponseErrorDto
{
    [JsonPropertyName("error")]
    public required ErrorDto Error { get; set; }
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