using System.Drawing;

namespace BabyBearsEngine.Source.Graphics.Text;
internal interface IFontBitmapGenerator
{
    GeneratedFontStruct GenerateCharSpritesheetAndPositions(Font font, string charsToLoad, bool antiAliased, int charactersPerRow);
}
