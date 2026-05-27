using System.Text.Json;
using System.Text.Json.Serialization;

namespace BabyBearsEngine.IO;

/// <summary>
/// Serializes <see cref="Colour"/> as a hex string (<c>"#RRGGBBAA"</c>).
/// Deserializes from either that hex form or an object form (<c>{"R":255,"G":0,"B":0,"A":255}</c>).
/// </summary>
public sealed class ColourJsonConverter : JsonConverter<Colour>
{
    public override Colour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string hex = reader.GetString() ?? throw new JsonException("Colour hex string was null.");
            return new Colour(hex);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            byte r = 0, g = 0, b = 0, a = 255;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propName = reader.GetString() ?? throw new JsonException("Property name was null.");
                reader.Read();
                switch (propName)
                {
                    case "R": r = reader.GetByte(); break;
                    case "G": g = reader.GetByte(); break;
                    case "B": b = reader.GetByte(); break;
                    case "A": a = reader.GetByte(); break;
                    default: reader.Skip(); break;
                }
            }
            return new Colour(r, g, b, a);
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when reading Colour.");
    }

    public override void Write(Utf8JsonWriter writer, Colour value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToHex());
    }
}
