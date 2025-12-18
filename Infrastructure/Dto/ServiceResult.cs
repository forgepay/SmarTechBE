using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Serialization;

namespace MicPic.Infrastructure.Dto;

public class ServiceResult : JsonResult
{
    public ServiceResult(object? value, int? statusCode) : base(
        value: value,
        serializerSettings: AppJsonSerializerOptions.Default)
    {
        StatusCode = statusCode;
    }

    public ServiceResult(EmptyResult emptyResult) : base(
        value: new ServiceResponseDto<object?>(null),
        serializerSettings: AppJsonSerializerOptions.Default)
    {

    }

    public ServiceResult(ObjectResult objectResult) : base(
        value: new ServiceResponseDto<object?>(objectResult?.Value) { Success = (objectResult?.StatusCode ?? 200) < 400 },
        serializerSettings: AppJsonSerializerOptions.Default)
    {
        if (objectResult is not null && objectResult.StatusCode.HasValue)
            StatusCode = objectResult.StatusCode;
    }

    public ServiceResult(ValidationProblemDetails validationProblemDetails) : base(
        value: new ServiceResponseDto<object?>(new AppValidationProblemDetails(validationProblemDetails)),
        serializerSettings: AppJsonSerializerOptions.Default)
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }

    public ServiceResult(Exception exception) : base(
        value: new ServiceResponseDto<object?>(exception),
        serializerSettings: AppJsonSerializerOptions.Default)
    {
        StatusCode = exception is AppException appException ?
            appException.StatusCode : StatusCodes.Status500InternalServerError;
    }
}