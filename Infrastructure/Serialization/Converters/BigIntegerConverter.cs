using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicPic.Infrastructure.Helpers;

namespace MicPic.Infrastructure.Serialization.Converters;

public class BigIntegerConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                using var doc = JsonDocument.ParseValue(ref reader);

                return BigInteger.Parse(doc.RootElement.GetRawText(), NumberFormatInfo.InvariantInfo);
            }

            case JsonTokenType.String:
            {
                using var doc = JsonDocument.ParseValue(ref reader);

                var str = doc.RootElement.GetRawText();
                
                var value = str.Trim().Trim('"').Trim();

                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && BigIntegerHelper.TryParseHexNumber(value, out var hexNumber))
                    return hexNumber;

                if (BigIntegerHelper.TryParseNumber(value, out var number))
                    return number;
                    
                throw new JsonException($"Value {str} can't be parsed as number.");
            }
            

            default:
                throw new JsonException($"Found token {reader.TokenType} but expected token {JsonTokenType.Number}");
        }
    }

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));

        writer.WriteRawValue(value.ToString(NumberFormatInfo.InvariantInfo), false);        
    }
}