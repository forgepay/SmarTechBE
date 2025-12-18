using System.Text.Json.Serialization;
using MicPic.Infrastructure.Exceptions;

namespace CryptoOnRamp.API.Models;

public class ServiceResponseDto<TResult>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("data")]
    public TResult? Data { get; set; }

    [JsonPropertyName("expandedError")]
    public ExtendedErrorResultDto? ExpandedError { get; set; }

    [Obsolete("Parameterless constructor should be used for deserialization only.")]
    public ServiceResponseDto()
    {

    }

    public ServiceResponseDto(TResult result)
    {
        if (result is AppException appException)
        {
            Success = false;
            ExpandedError =
                new ExtendedErrorResultDto
                {
                    ErrorCode = appException.StatusCode,
                    Message = appException.Message,
                    TechnicalMessage = appException.TechnicalMessage,
                };
        }
        else if (result is Exception exception)
        {
            Success = false;
            ExpandedError =
                new ExtendedErrorResultDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    Message = "Something went wrong",
                    TechnicalMessage = exception.Message,
                };
        }
        else
        {
            Success = true;
            Data = result;
        }
    }

    public class ExtendedErrorResultDto
    {

        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("technicalMessage")]
        public string? TechnicalMessage { get; set; }
    }
}
