using CryptoOnRamp.API.Models;
using MicPic.Infrastructure.Exceptions;
using System.Net.Mime;
using System.Text.Json;

namespace CryptoOnRamp.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (AppException ex)
        {
            var content = JsonSerializer.Serialize(new ServiceResponseDto<object>(ex));

            var response = httpContext.Response;

            response.StatusCode = ex.StatusCode;
            response.ContentType = MediaTypeNames.Application.Json;

            await response.WriteAsync(content);

            _logger.LogWarning(ex.Message);
        }
        catch (Exception ex)
        {
            var content = JsonSerializer.Serialize(new ServiceResponseDto<object>(ex));

            var response = httpContext.Response;

            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.ContentType = MediaTypeNames.Application.Json;

            await response.WriteAsync(content);

            _logger.LogError(ex, "");
        }
    }

    public class ErrorModelResponseBase
    {
        public int Code { get; set; }
        public string DisplayMessage { get; set; } = string.Empty;
    }
}
