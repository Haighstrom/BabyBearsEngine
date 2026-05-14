using System.Drawing;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal interface IFontLoader
{
    Font LoadFont(FontDefinition fontDef);

    Font LoadFont(string fontName, float size, FontStyle style);
}
