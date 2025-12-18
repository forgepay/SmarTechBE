using System.Net;
using MicPic.Infrastructure.Extensions;

namespace MicPic.Infrastructure.RateLimit.Internal;

internal static class RateLimitOptionsExtensions
{
    public static bool IsWhitelisted(this RateLimitOptions options, string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        foreach (var range in options.WhiteIpList)
        {
            if (ip.IsInRange(range))
                return true;
        }

        return false;
    }
}