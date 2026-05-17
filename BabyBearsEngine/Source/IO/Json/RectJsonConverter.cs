using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.IO;

/// <summary>Serializes <see cref="Rect"/> as <c>{"X":0,"Y":0,"W":100,"H":50}</c>.</summary>
public sealed class RectJsonConverter : JsonConverter<Rect>
{
    public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected start of object, got {reader.TokenType}.");
        }

        float x = 0f, y = 0f, w = 0f, h = 0f;
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
                case "W": w = reader.GetSingle(); break;
                case "H": h = reader.GetSingle(); break;
                default: reader.Skip(); break;
            }
        }
        return new Rect(x, y, w, h);
    }

    public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("W", value.W);
        writer.WriteNumber("H", value.H);
        writer.WriteEndObject();
    }
}
