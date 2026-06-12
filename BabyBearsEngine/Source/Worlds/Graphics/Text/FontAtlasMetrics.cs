using OpenTK.Mathematics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Per-character metrics for a font atlas — widest/tallest character dimensions
/// plus UV positions for each glyph within the atlas texture. The texture itself
/// is held separately by the <see cref="CachedFontAtlas"/> that owns these metrics,
/// so this record is backend-agnostic: a GDI+ bitmap atlas, an SDF atlas, or any
/// future generator can produce the same shape.
/// </summary>
/// <param name="CharAdvances">
/// Optional per-glyph horizontal advance (logical pixels). When supplied, the pen moves on by
/// this amount after a glyph — decoupled from the render-quad width, which an SDF atlas makes
/// wider than the advance so the distance-field "glow" margins render instead of being clipped.
/// When null, the advance falls back to the render-quad width (the GDI+ atlas's behaviour, where
/// the bitmap width already <em>is</em> the advance).
/// </param>
/// <param name="CharBearings">
/// Optional per-glyph offset (logical pixels) of the render quad's top-left from the pen position
/// and line top. Lets the glow-inclusive quad sit outside the advance box — left/above the glyph —
/// so margins are not clipped. When null, the quad is placed at the pen with no offset (GDI+).
/// </param>
internal record class FontAtlasMetrics(
    int WidestChar,
    int HighestChar,
    Dictionary<char, Box2i> CharPositions,
    Dictionary<char, Box2> CharPositionsNormalised,
    Dictionary<char, int>? CharAdvances = null,
    Dictionary<char, Vector2i>? CharBearings = null)
{
    /// <summary>
    /// Horizontal pen advance for a glyph, in logical pixels. Falls back to the render-quad width
    /// when no explicit advance map was supplied (GDI+ atlas).
    /// </summary>
    public int GetCharAdvance(char c)
    {
        if (CharAdvances is not null && CharAdvances.TryGetValue(c, out int advance))
        {
            return advance;
        }

        return GetCharPosition(c).Size.X;
    }

    /// <summary>
    /// Offset of a glyph's render quad from the pen/line-top, in logical pixels. Zero when no
    /// bearing map was supplied (GDI+ atlas), placing the quad directly at the pen.
    /// </summary>
    public Vector2i GetCharBearing(char c)
    {
        if (CharBearings is not null && CharBearings.TryGetValue(c, out Vector2i bearing))
        {
            return bearing;
        }

        return Vector2i.Zero;
    }

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

    public Vector2i MeasureString(char c) => new(GetCharAdvance(c), HighestChar);

    public Vector2i MeasureString(string s) => MeasureString(s.AsSpan());

    public Vector2i MeasureString(ReadOnlySpan<char> s)
    {
        int length = 0;

        foreach (char c in s)
        {
            length += GetCharAdvance(c);
        }

        return new(length, HighestChar);
    }
}
