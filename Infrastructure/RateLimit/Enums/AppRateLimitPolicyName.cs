namespace MicPic.Infrastructure.RateLimit.Enums;

public static class AppRateLimitPolicyName
{
    public const string Default = "Default";
    public const string PerIp = "PerIp";
    public const string PerUser = "PerUser";
    public const string Concurrent = "Concurrent";
}
