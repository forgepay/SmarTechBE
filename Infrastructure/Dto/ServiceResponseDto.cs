using System.Text.Json.Serialization;
using MicPic.Infrastructure.Exceptions;

namespace MicPic.Infrastructure.Dto;

public sealed record ServiceResponseDto<TResult>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("data")]
    public TResult? Data { get; set; }

    [JsonPropertyName("error")]
    public ErrorResultDto? Error { get; set; }

    [Obsolete("Parameterless constructor should be used for deserialization only.")]
    public ServiceResponseDto()
    {

    }

    public ServiceResponseDto(TResult result)
    {
        if (result is AppValidationProblemDetails)
        {
            Success = false;
            Data = result;
            Error =
                new ErrorResultDto
                {
                    ErrorCode = BusinessErrorCodes.ValidationError,
                    Message = BusinessErrorMessages.GetErrorMessage(BusinessErrorCodes.ValidationError),
                };
        }
        else if (result is AppException appException)
            {
                Success = false;
                Error =
                    new ErrorResultDto
                    {
                        ErrorCode = appException.Code,
                        Message = appException.Message,
                        TechnicalMessage = appException.TechnicalMessage,
                        TechnicalData = appException.Data.Count > 0 ? appException.Data : null,
                    };
            }
            else if (result is Exception)
            {
                Success = false;
                Error =
                    new ErrorResultDto
                    {
                        ErrorCode = BusinessErrorCodes.Unexpected,
                        Message = BusinessErrorMessages.GetErrorMessage(BusinessErrorCodes.Unexpected)
                    };
            }
            else
            {
                Success = true;
                Data = result;
            }
    }
}
