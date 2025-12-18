using System.Threading.RateLimiting;
using MicPic.Infrastructure.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.RateLimit.Enums;
using MicPic.Infrastructure.RateLimit.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace MicPic.Infrastructure.RateLimit.Policies;

internal sealed class AppPerIpRateLimitPolicy(IOptions<RateLimitOptions> options) : IRateLimiterPolicy<string>
{
    private readonly RateLimitOptions _options = options.Value;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var ipAddress = GetClientIpAddress(httpContext);

        if (_options.IsWhitelisted(ipAddress))
        {
            return RateLimitPartition.GetNoLimiter(ipAddress);
        }

        if (!_options.Policies.TryGetValue(AppRateLimitPolicyName.PerIp, out var policyOptions))
            throw new AppException("Rate limit policy not configured", BusinessErrorCodes.Misconfigured);

        return RateLimitPartition
            .GetSlidingWindowLimiter(
                partitionKey: ipAddress,
                factory: options =>
                    new SlidingWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = policyOptions.MaxRequests,
                        Window = policyOptions.Window,
                        QueueLimit = policyOptions.MaxQueue,
                        SegmentsPerWindow = (int)policyOptions.Window.TotalSeconds,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    });
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = OnRejectedFunction;


    #region Private Methods

    private static ValueTask OnRejectedFunction(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;

        response.StatusCode = StatusCodes.Status429TooManyRequests;
        response
            .WriteAsJsonAsync(
                value: new ServiceResponseDto<AppException>(
                    result: new AppException("Rate limit exceeded", BusinessErrorCodes.RateLimitExceeded)),
                cancellationToken: cancellationToken);        

        return ValueTask.CompletedTask;
    }

    private static string GetClientIpAddress(HttpContext httpContext)
    {
        // Check X-Forwarded-For header (most common)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Fallback to X-Real-IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Final fallback to RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }    

    #endregion
}
