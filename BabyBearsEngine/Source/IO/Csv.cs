using System.Globalization;
using System.Text;

namespace BabyBearsEngine.IO;

/// <summary>
/// Static facade for CSV serialization and deserialization. Fields are escaped per RFC 4180:
/// a field containing the separator, a double-quote, or a line break is wrapped in double-quotes,
/// and any embedded double-quotes are doubled.
/// </summary>
public static class Csv
{
    /// <summary>
    /// Serializes a 2D array to a CSV string. Values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static string Serialize<T>(T[,] data, char separator = ',') where T : IConvertible
    {
        int rowCount = data.GetLength(0);
        int colCount = data.GetLength(1);
        StringBuilder builder = new();

        for (int row = 0; row < rowCount; row++)
        {
            if (row > 0)
            {
                builder.Append('\n');
            }

            for (int col = 0; col < colCount; col++)
            {
                if (col > 0)
                {
                    builder.Append(separator);
                }

                string formatted = ((IConvertible)data[row, col]).ToString(CultureInfo.InvariantCulture);
                AppendField(builder, formatted, separator);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Deserializes a CSV string to a 2D array of <typeparamref name="T"/>. Values are converted
    /// using <see cref="CultureInfo.InvariantCulture"/>. Rows with fewer columns than the widest
    /// row are padded with <c>default(T)</c>.
    /// </summary>
    public static T[,] Deserialize<T>(string csv, char separator = ',') where T : IConvertible
    {
        List<List<string>> rows = ParseRecords(csv, separator);
        return BuildArray<T>(rows);
    }

    /// <summary>
    /// Serializes a 2D array to a CSV string, prepending <paramref name="headers"/> as the first row.
    /// </summary>
    public static string SerializeWithHeader<T>(IEnumerable<string> headers, T[,] data, char separator = ',') where T : IConvertible
    {
        string[] headerArray = [.. headers];
        int colCount = data.GetLength(1);

        if (headerArray.Length != colCount)
        {
            throw new ArgumentException(
                $"Header count ({headerArray.Length}) does not match column count ({colCount}).",
                nameof(headers));
        }

        StringBuilder builder = new();

        for (int col = 0; col < headerArray.Length; col++)
        {
            if (col > 0)
            {
                builder.Append(separator);
            }

            AppendField(builder, headerArray[col], separator);
        }

        if (data.GetLength(0) > 0)
        {
            builder.Append('\n');
            builder.Append(Serialize(data, separator));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Deserializes a CSV string with a header row, returning the headers and data separately.
    /// The first parsed row is treated as the header. If the input is empty, both arrays are empty.
    /// </summary>
    public static (string[] Headers, T[,] Data) DeserializeWithHeader<T>(string csv, char separator = ',') where T : IConvertible
    {
        List<List<string>> rows = ParseRecords(csv, separator);

        if (rows.Count == 0)
        {
            return ([], new T[0, 0]);
        }

        string[] headers = [.. rows[0]];
        List<List<string>> dataRows = [.. rows.Skip(1)];
        T[,] data = BuildArray<T>(dataRows);
        return (headers, data);
    }

    // Internals

    private static void AppendField(StringBuilder builder, string value, char separator)
    {
        bool needsQuoting = value.Contains(separator)
            || value.Contains('"')
            || value.Contains('\n')
            || value.Contains('\r');

        if (!needsQuoting)
        {
            builder.Append(value);
            return;
        }

        builder.Append('"');
        foreach (char ch in value)
        {
            if (ch == '"')
            {
                builder.Append('"');
            }
            builder.Append(ch);
        }
        builder.Append('"');
    }

    private static List<List<string>> ParseRecords(string csv, char separator)
    {
        List<List<string>> rows = [];

        if (string.IsNullOrEmpty(csv))
        {
            return rows;
        }

        List<string> currentRow = new();
        StringBuilder field = new();
        bool inQuotes = false;
        int length = csv.Length;

        for (int position = 0; position < length; position++)
        {
            char ch = csv[position];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (position + 1 < length && csv[position + 1] == '"')
                    {
                        field.Append('"');
                        position++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(ch);
                }
            }
            else
            {
                if (ch == '"' && field.Length == 0)
                {
                    inQuotes = true;
                }
                else if (ch == separator)
                {
                    currentRow.Add(field.ToString());
                    field.Clear();
                }
                else if (ch == '\r')
                {
                    if (position + 1 < length && csv[position + 1] == '\n')
                    {
                        position++;
                    }
                    currentRow.Add(field.ToString());
                    field.Clear();
                    rows.Add(currentRow);
                    currentRow = new();
                }
                else if (ch == '\n')
                {
                    currentRow.Add(field.ToString());
                    field.Clear();
                    rows.Add(currentRow);
                    currentRow = new();
                }
                else
                {
                    field.Append(ch);
                }
            }
        }

        currentRow.Add(field.ToString());
        if (currentRow.Count > 1 || currentRow[0].Length > 0)
        {
            rows.Add(currentRow);
        }

        return rows;
    }

    private static T[,] BuildArray<T>(List<List<string>> rows) where T : IConvertible
    {
        int rowCount = rows.Count;
        if (rowCount == 0)
        {
            return new T[0, 0];
        }

        int colCount = rows.Max(r => r.Count);
        T[,] result = new T[rowCount, colCount];

        for (int row = 0; row < rowCount; row++)
        {
            List<string> source = rows[row];
            for (int col = 0; col < source.Count; col++)
            {
                result[row, col] = (T)Convert.ChangeType(source[col], typeof(T), CultureInfo.InvariantCulture);
            }
        }

        return result;
    }
}
