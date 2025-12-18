using System.Diagnostics;
using System.Text;

namespace MicPic.Infrastructure.Extensions;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1308 // Normalize strings to uppercase

public static class ByteArrayExtensions
{
    [DebuggerStepThrough]
    public static string ToHexString(this byte[] array)
    {
        return BitConverter
            .ToString(array)
            .Replace("-", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();
    }

    public static string To0xHexString(this byte[] array)
        => array.Length != 0 ? string.Concat("0x", array.ToHexString()) : string.Empty;

    [DebuggerStepThrough]
    public static string GetUTF8(this ReadOnlyMemory<byte> body)
    {
        try
        {
            return Encoding.UTF8.GetString(body.Span);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static byte[]? NullIfZeroLength(this byte[] array)
        => array.Length != 0 ? array : null;
}
