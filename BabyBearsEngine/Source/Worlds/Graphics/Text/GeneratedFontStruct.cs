using System.Collections.Generic;
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

        throw new InvalidOperationException(
            $"Character '{c}' (U+{(int)c:X4}) is not loaded in this font atlas. " +
            $"Add it to FontDefinition.ExtraCharactersToLoad.");
    }

    public Box2 GetCharPositionNormalised(char c)
    {
        if (CharPositionsNormalised.TryGetValue(c, out Box2 pos))
        {
            return pos;
        }

        throw new InvalidOperationException(
            $"Character '{c}' (U+{(int)c:X4}) is not loaded in this font atlas. " +
            $"Add it to FontDefinition.ExtraCharactersToLoad.");
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
