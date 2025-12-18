namespace MicPic.Infrastructure.RateLimit.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AppRateLimitPolicyAttribute(string policyName) : Attribute
{
    public string PolicyName { get; } = policyName;

    public int Priority { get; set; }
}
