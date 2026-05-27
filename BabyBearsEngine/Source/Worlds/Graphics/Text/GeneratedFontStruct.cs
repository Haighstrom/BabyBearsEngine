using System.Drawing;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal record class GeneratedFontStruct(Bitmap CharacterSS, int WidestChar, int HighestChar, Dictionary<char, Box2i> CharPositions, Dictionary<char, Box2> CharPositionsNormalised)
{
    public Box2i GetCharPosition(char c)
    {
        if (CharPositions.TryGetValue(c, out Box2i pos))
        {
            return pos;
        }

        throw CharNotLoaded(c);
    }

    public Box2 GetCharPositionNormalised(char c)
    {
        if (CharPositionsNormalised.TryGetValue(c, out Box2 pos))
        {
            return pos;
        }

        throw CharNotLoaded(c);
    }

    /// <summary>
    /// Builds the exception thrown when a character is requested that the font atlas does not contain.
    /// Control characters are named (e.g. "newline") because their raw glyph is invisible in a message.
    /// </summary>
    private static InvalidOperationException CharNotLoaded(char c)
    {
        return new InvalidOperationException(
            $"Character {DescribeChar(c)} is not loaded in this font atlas. " +
            $"Add it to FontDefinition.ExtraCharactersToLoad.");
    }

    /// <summary>
    /// Produces a human-readable description of a character for error messages — a name for
    /// non-printable characters, the glyph itself otherwise, always with the Unicode code point.
    /// </summary>
    private static string DescribeChar(char c)
    {
        string label = c switch
        {
            '\n' => "newline",
            '\r' => "carriage return",
            '\t' => "tab",
            ' ' => "space",
            _ when char.IsControl(c) => "control character",
            _ => $"'{c}'",
        };

        return $"{label} (U+{(int)c:X4})";
    }

    public Vector2i MeasureString(char c) => new(GetCharPosition(c).Size.X, HighestChar);

    public Vector2i MeasureString(string s)
    {
        int length = 0;

        foreach (char c in s)
        {
            length += GetCharPosition(c).Size.X;
        }

        return new(length, HighestChar);
    }
}
