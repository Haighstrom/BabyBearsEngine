using System.Collections.Generic;
using System.Linq;

namespace BabyBearsEngine.IO;

/// <summary>
/// Static facade for plain-text serialization. Provided for API symmetry with
/// <see cref="Json"/>, <see cref="Xml"/>, and <see cref="Csv"/>.
/// </summary>
public static class Txt
{
    /// <summary>Joins <paramref name="lines"/> with <c>'\n'</c> into a single string.</summary>
    public static string Serialize(IEnumerable<string> lines) =>
        string.Join('\n', lines);

    /// <summary>
    /// Splits <paramref name="text"/> into lines on any line break (<c>\r\n</c>, <c>\n</c>, or <c>\r</c>).
    /// Returns an empty array if the input is null or empty.
    /// </summary>
    public static string[] Deserialize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        return [.. text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None)];
    }
}
