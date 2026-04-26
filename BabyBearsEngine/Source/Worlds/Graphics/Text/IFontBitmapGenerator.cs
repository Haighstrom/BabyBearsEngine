using System.Drawing;

namespace BabyBearsEngine.Rendering.Graphics.Text;
internal interface IFontBitmapGenerator
{
    GeneratedFontStruct GenerateCharSpritesheetAndPositions(Font font, string charsToLoad, bool antiAliased, int charactersPerRow);
}
