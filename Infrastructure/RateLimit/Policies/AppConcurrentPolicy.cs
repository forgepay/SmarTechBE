using System.Threading.RateLimiting;
using MicPic.Infrastructure.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.RateLimit.Enums;
using MicPic.Infrastructure.RateLimit.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace MicPic.Infrastructure.RateLimit.Policies;

internal sealed class AppConcurrentPolicy(IOptions<RateLimitOptions> options) : IRateLimiterPolicy<string>
{
    private readonly RateLimitOptions _options = options.Value;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        if (!_options.Policies.TryGetValue(AppRateLimitPolicyName.Concurrent, out var policyOptions))
            throw new AppException("Rate limit policy not configured", BusinessErrorCodes.Misconfigured);

        return RateLimitPartition
            .GetConcurrencyLimiter(
                partitionKey: "global",
                factory: options =>
                    new ConcurrencyLimiterOptions
                    {
                        PermitLimit = policyOptions.MaxRequests,
                        QueueLimit = policyOptions.MaxQueue,
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
                    result: new AppException("Waiting queue has no space", BusinessErrorCodes.RateLimitExceeded)),
                cancellationToken: cancellationToken);        

        return ValueTask.CompletedTask;
    }

    #endregion
}
