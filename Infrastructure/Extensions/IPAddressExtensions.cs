using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace MicPic.Infrastructure.Extensions;

public static class IPAddressExtensions
{
    [DebuggerStepThrough]
    public static bool IsInRange(this IPAddress address, string cidr)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentException.ThrowIfNullOrEmpty(cidr);

        var parts = cidr.Split('/');
        var baseAddress = IPAddress.Parse(parts[0]);
        var prefixLength = int.Parse(parts[1], CultureInfo.InvariantCulture);

        var baseBytes = baseAddress.GetAddressBytes();
        var addressBytes = address.GetAddressBytes();

        if (baseBytes.Length != addressBytes.Length)
            return false;

        var maskBytes = prefixLength / 8;
        var maskBits = prefixLength % 8;

        for (int i = 0; i < maskBytes; i++)
        {
            if (baseBytes[i] != addressBytes[i])
                return false;
        }

        if (maskBits > 0)
        {
            var mask = (byte)(0xFF << (8 - maskBits));
            if ((baseBytes[maskBytes] & mask) != (addressBytes[maskBytes] & mask))
                return false;
        }

        return true;
    }
}
