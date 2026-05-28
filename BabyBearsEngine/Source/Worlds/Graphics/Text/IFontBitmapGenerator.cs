using System.Drawing;

namespace BabyBearsEngine.Worlds.Graphics.Text;
internal interface IFontBitmapGenerator
{
    (Bitmap Bitmap, FontAtlasMetrics Metrics) GenerateCharSpritesheetAndPositions(Font font, string charsToLoad, bool antiAliased, int charactersPerRow);
}
