using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicPic.Infrastructure.Serialization.Converters;

// Custom converter for raw JSON (supports arrays and objects)
public sealed class RawJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the raw JSON value (object or array) as a string
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        return jsonDoc.RootElement.GetRawText();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // Write the raw JSON string back as a JSON object or array
        using var jsonDoc = JsonDocument.Parse(value);
        jsonDoc.RootElement.WriteTo(writer);
    }
}