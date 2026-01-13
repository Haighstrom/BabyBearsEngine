using System.Collections.Generic;
using System.Drawing;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

internal record class GeneratedFontStruct(Bitmap CharacterSS, int WidestChar, int HighestChar, Dictionary<char, Box2i> CharPositions, Dictionary<char, Box2> CharPositionsNormalised)
{
    public Vector2i MeasureString(char c) => new(CharPositions[c].Size.X, HighestChar);

    public Vector2i MeasureString(string s)
    {
        int length = 0;

        foreach (char c in s)
        {
            length += CharPositions[c].Size.X;
        }

        return new(length, HighestChar);
    }
}
