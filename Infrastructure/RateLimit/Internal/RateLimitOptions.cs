using System.Collections.ObjectModel;
using MicPic.Infrastructure.RateLimit.Enums;

namespace MicPic.Infrastructure.RateLimit.Internal;

internal sealed record RateLimitOptions
{
    public const string Position = "RateLimit";

    public Dictionary<string, RateLimitPolicyOptions> Policies { get; init; } =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [AppRateLimitPolicyName.Default] =
                new()
                {
                    MaxRequests = 180,
                    Window = TimeSpan.FromSeconds(60),
                },
            
            [AppRateLimitPolicyName.PerIp] =
                new()
                {
                    MaxRequests = 30,
                    Window = TimeSpan.FromSeconds(60),
                },

            [AppRateLimitPolicyName.PerUser] =
                new()
                {
                    MaxRequests = 30,
                    Window = TimeSpan.FromSeconds(60),
                },

            [AppRateLimitPolicyName.Concurrent] =
                new()
                {
                    MaxRequests = 1,
                    MaxQueue = 2,
                },
        };

    public Collection<string> WhiteIpList { get; init; } = [];
}
