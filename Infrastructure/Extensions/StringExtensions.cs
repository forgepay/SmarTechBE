using System.Diagnostics;

namespace MicPic.Infrastructure.Extensions;

#pragma warning disable CA1062 // Validate arguments of public methods

public static class StringExtensions
{
    public static string? NullIfWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    public static string? NullIfNone(this string? value) =>
        string.Equals("none", value, StringComparison.OrdinalIgnoreCase) ? null : value;

    public static string? Replace(this string? value, Dictionary<string, string> replacements)
    {
        if (value is null) return null;

        ArgumentNullException.ThrowIfNull(replacements);

        foreach (var (key, replacement) in replacements)
        {
            value = value.Replace(key, replacement, StringComparison.OrdinalIgnoreCase);
        }

        return value;
    }

    [DebuggerStepThrough]
    public static string Chop0x(this string value)
        => value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;

    [DebuggerStepThrough]
    public static byte[] HexToByteArray(this string value)
        => value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?
            HexToByteArray(value.Substring(2)) :
            Enumerable.Range(0, value.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                .ToArray();

    [DebuggerStepThrough]
    public static string HexToBase64(this string value)
        => Convert.ToBase64String(value.HexToByteArray());

    [DebuggerStepThrough]
    public static string Base64To0xHex(this string value)
        => Convert.FromBase64String(value).To0xHexString();    
}
