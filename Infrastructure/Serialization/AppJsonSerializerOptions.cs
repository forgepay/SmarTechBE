using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicPic.Infrastructure.Serialization.Converters;

namespace MicPic.Infrastructure.Serialization;

public static class AppJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = ConfigureDefault(new JsonSerializerOptions());

    public static JsonSerializerOptions ConfigureDefault(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.PropertyNameCaseInsensitive = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new BigIntegerConverter());

        return options;
    }
}
