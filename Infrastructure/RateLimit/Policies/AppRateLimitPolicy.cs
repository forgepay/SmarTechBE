using System.Threading.RateLimiting;
using MicPic.Infrastructure.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.RateLimit.Enums;
using MicPic.Infrastructure.RateLimit.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace MicPic.Infrastructure.RateLimit.Policies;

internal sealed class AppRateLimitPolicy(IOptions<RateLimitOptions> options) : IRateLimiterPolicy<string>
{
    private readonly RateLimitOptions _options = options.Value;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        var routePattern = endpoint?.Metadata.GetMetadata<RouteEndpoint>()?.RoutePattern?.RawText;
        var method = httpContext.Request.Method;
        var endpointKey = $"{method}:{routePattern}";

        if (!_options.Policies.TryGetValue(AppRateLimitPolicyName.Default, out var policyOptions))
            throw new AppException("Rate limit policy not configured", BusinessErrorCodes.Misconfigured);

        return RateLimitPartition
            .GetSlidingWindowLimiter(
                partitionKey: endpointKey,
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

        response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        response
            .WriteAsJsonAsync(
                value: new ServiceResponseDto<AppException>(
                    result: new AppException("Rate limit exceeded", BusinessErrorCodes.RateLimitExceeded)),
                cancellationToken: cancellationToken);        

        return ValueTask.CompletedTask;
    }

    #endregion
}
