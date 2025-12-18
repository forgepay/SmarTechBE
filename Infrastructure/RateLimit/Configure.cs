using MicPic.Infrastructure.RateLimit.Enums;
using MicPic.Infrastructure.RateLimit.Internal;
using MicPic.Infrastructure.RateLimit.Middlewares;
using MicPic.Infrastructure.RateLimit.Policies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicPic.Infrastructure.RateLimit;

public static class Configure
{
    public static IServiceCollection AddAppRateLimit(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        // Options
        
        services.Configure<RateLimitOptions>(configuration
            .GetSection(RateLimitOptions.Position));

        // Services

        services.AddKeyedSingleton<IRateLimiterPolicy<string>, AppRateLimitPolicy>(AppRateLimitPolicyName.Default);
        services.AddKeyedSingleton<IRateLimiterPolicy<string>, AppPerIpRateLimitPolicy>(AppRateLimitPolicyName.PerIp);
        services.AddKeyedSingleton<IRateLimiterPolicy<string>, AppPerUserRateLimitPolicy>(AppRateLimitPolicyName.PerUser);
        services.AddKeyedSingleton<IRateLimiterPolicy<string>, AppConcurrentPolicy>(AppRateLimitPolicyName.Concurrent);

        return services;
    }

    public static IApplicationBuilder UseAppRateLimit(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<RateLimitMiddleware>();
    }
}
