using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BabyBearsEngine.IO;

public class TwoDimensionalArrayConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsArray && typeToConvert.GetArrayRank() == 2;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Non-null: GetElementType() only returns null for non-array types; CanConvert guarantees typeToConvert is a 2D array.
        var elementType = typeToConvert.GetElementType()!;

        // Non-null: CreateInstance throws on failure and only returns null for Nullable<T>; TwoDimensionalArrayConverterInner<T> is a concrete class.
        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(TwoDimensionalArrayConverterInner<>).MakeGenericType(elementType))!;

        return converter;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection in CreateConverter.")]
    private class TwoDimensionalArrayConverterInner<TElement> : JsonConverter<TElement[,]>
    {
        public override TElement[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            var rows = new List<TElement[]>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                var columns = new List<TElement>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    var element = JsonSerializer.Deserialize<TElement>(ref reader, options)
                        ?? throw new JsonException("Unexpected null element in 2D array.");
                    columns.Add(element);
                }

                rows.Add(columns.ToArray());
            }

            return ConvertToTwoDimensionalArray(rows);
        }

        public override void Write(Utf8JsonWriter writer, TElement[,] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            for (int i = 0; i < value.GetLength(0); i++)
            {
                writer.WriteStartArray();
                for (int j = 0; j < value.GetLength(1); j++)
                {
                    var element = value[i, j];
                    JsonSerializer.Serialize(writer, element, options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }

        private static TElement[,] ConvertToTwoDimensionalArray(List<TElement[]> list)
        {
            int rowCount = list.Count;
            int columnCount = list.Max(x => x.Length);

            var result = new TElement[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                var row = list[i];
                for (int j = 0; j < row.Length; j++)
                {
                    result[i, j] = row[j];
                }
            }
            return result;
        }
    }
}
