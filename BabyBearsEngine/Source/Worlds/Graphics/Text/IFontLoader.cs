using System.Drawing;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

internal interface IFontLoader
{
    Font LoadFont(FontDefinition fontDef);

    Font LoadFont(string fontName, float size, FontStyle style);
}
