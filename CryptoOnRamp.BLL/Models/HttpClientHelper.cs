using System.Globalization;
using System.Net;

namespace CryptoOnRamp.BLL.Models;

public static class HttpClientHelper
{
    public static string ToQueryString(this IEnumerable<KeyValuePair<string, object?>> collection)
  => string.Join('&', collection.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(FormatUrlArg(x.Value))}"));

    public static string ToQueryString(this IEnumerable<KeyValuePair<string, string>> collection)
        => string.Join('&', collection.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(FormatUrlArg(x.Value))}"));

    public static string FormatUrlArg(object? arg)
    {
        if (arg is null)
            return string.Empty;
        if (arg is bool _bool)
            return _bool.ToInvariantString();
        if (arg is int _int)
            return _int.ToInvariantString();
        if (arg is decimal _decimal)
            return _decimal.ToInvariantString();
        return arg.ToString() ?? string.Empty;
    }

    public static string ToInvariantString(this decimal value) => value.ToString(CultureInfo.InvariantCulture);
    public static string ToInvariantString(this bool value) => value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

    public static string ToInvariantString(this int value) => value.ToString(CultureInfo.InvariantCulture);
}
