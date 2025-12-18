using System.Globalization;
using System.Threading.RateLimiting;
using MicPic.Infrastructure.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Extensions;
using MicPic.Infrastructure.RateLimit.Enums;
using MicPic.Infrastructure.RateLimit.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace MicPic.Infrastructure.RateLimit.Policies;

internal sealed class AppPerUserRateLimitPolicy(IOptions<RateLimitOptions> options) : IRateLimiterPolicy<string>
{
    private readonly RateLimitOptions _options = options.Value;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var userId = httpContext.User.Id().ToString(CultureInfo.InvariantCulture);

        if (!_options.Policies.TryGetValue(AppRateLimitPolicyName.PerIp, out var policyOptions))
            throw new AppException("Rate limit policy not configured", BusinessErrorCodes.Misconfigured);

        return RateLimitPartition
            .GetSlidingWindowLimiter(
                partitionKey: userId,
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

    #endregion
}
