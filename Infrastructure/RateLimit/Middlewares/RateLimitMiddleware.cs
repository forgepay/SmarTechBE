using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using MicPic.Infrastructure.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.RateLimit.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace MicPic.Infrastructure.RateLimit.Middlewares;

public sealed class RateLimitMiddleware(
    RequestDelegate next,
    IServiceProvider serviceProvider) : IDisposable
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ConcurrentDictionary<string, PartitionedRateLimiter<HttpContext>> _partitionedRateLimiters = new();

    public async Task Invoke(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var endpoint = httpContext.GetEndpoint();

        var attributes = endpoint?
            .Metadata
            .GetOrderedMetadata<AppRateLimitPolicyAttribute>() ?? [];

        var policies = attributes
            .OrderByDescending(a => a.Priority)
            .ToArray();

        await Invoke(httpContext, policies);
    }


    #region IDisposable Members

    public void Dispose()
    {
        foreach (var rateLimiter in _partitionedRateLimiters.Values)
        {
            rateLimiter.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    #endregion


    #region Private Methods

    private async Task Invoke(HttpContext httpContext, ArraySegment<AppRateLimitPolicyAttribute> policies)
    {
        if (policies.Count == 0)
        {
            await _next(httpContext);
        }
        else
        {
            var rateLimiter = GetPartitionedRateLimiter(policies[0].PolicyName);

            using var lease = await rateLimiter
                .AcquireAsync(httpContext, 1, httpContext.RequestAborted);

            if (lease.IsAcquired)
            {
                await Invoke(httpContext, policies.Slice(1));
            }
            else
            {
                var policy = _serviceProvider
                    .GetRequiredKeyedService<IRateLimiterPolicy<string>>(policies[0].PolicyName);

                if (policy.OnRejected != null)
                {
                    var context = new OnRejectedContext
                    {
                        HttpContext = httpContext,
                        Lease = lease
                    };

                    await policy.OnRejected(context, httpContext.RequestAborted);
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    await httpContext.Response
                        .WriteAsJsonAsync(
                            value: new ServiceResponseDto<AppException>(
                                result: new AppException("Rate limit exceeded", BusinessErrorCodes.RateLimitExceeded)),
                            cancellationToken: httpContext.RequestAborted);
                }
            }
        }
    }

    private PartitionedRateLimiter<HttpContext> GetPartitionedRateLimiter(string name)
    {
        var partitionedRateLimiter = _partitionedRateLimiters
            .GetOrAdd(name, CreatePartitionedRateLimiter);

        return partitionedRateLimiter;
    }

    private PartitionedRateLimiter<HttpContext> CreatePartitionedRateLimiter(string name)
    {
        var policy = _serviceProvider
            .GetRequiredKeyedService<IRateLimiterPolicy<string>>(name);

        return PartitionedRateLimiter
            .Create<HttpContext, string>(policy.GetPartition);
    }

    #endregion
}