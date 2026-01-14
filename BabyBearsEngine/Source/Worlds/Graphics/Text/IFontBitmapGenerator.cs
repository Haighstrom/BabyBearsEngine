using System.Drawing;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;
internal interface IFontBitmapGenerator
{
    GeneratedFontStruct GenerateCharSpritesheetAndPositions(Font font, string charsToLoad, bool antiAliased, int charactersPerRow);
}
