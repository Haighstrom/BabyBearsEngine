using System.Text.Json;
using System.Text.Json.Serialization;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.IO;

/// <summary>Serializes <see cref="Point"/> as <c>{"X":1.5,"Y":2.5}</c>.</summary>
public sealed class PointJsonConverter : JsonConverter<Point>
{
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected start of object, got {reader.TokenType}.");
        }

        float x = 0f, y = 0f;
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
                case "X": x = reader.GetSingle(); break;
                case "Y": y = reader.GetSingle(); break;
                default: reader.Skip(); break;
            }
        }
        return new Point(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}
