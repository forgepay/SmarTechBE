namespace MicPic.Infrastructure.RateLimit.Internal;

internal sealed record RateLimitPolicyOptions
{
    public int MaxRequests { get; init; }
    public TimeSpan Window { get; init; }
    public int MaxQueue { get; init; }
}
