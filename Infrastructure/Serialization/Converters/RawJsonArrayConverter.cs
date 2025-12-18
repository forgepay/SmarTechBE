using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicPic.Infrastructure.Serialization.Converters;

// Custom converter for an array of raw JSON strings
public sealed class RawJsonArrayConverter : JsonConverter<string[]>
{
    public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var rawJsonArray = new List<string>();

        // Parse the array
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            // Read each element in the array as raw JSON
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            rawJsonArray.Add(jsonDoc.RootElement.GetRawText());
        }

        return [.. rawJsonArray];
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        
        writer.WriteStartArray();

        foreach (var json in value)
        {
            // Write each string as raw JSON
            using var jsonDoc = JsonDocument.Parse(json);
            jsonDoc.RootElement.WriteTo(writer);
        }

        writer.WriteEndArray();
    }
}