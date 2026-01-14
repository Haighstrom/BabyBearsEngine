using System.Drawing;
using System.Drawing.Text;
using System.IO;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
internal class FontLoader() : IFontLoader
{
    private const string DEFAULT_FONT_FOLDER = "Assets/Fonts/";

    public Font LoadFont(FontDefinition fontDef) => LoadFont(fontDef.FontName, fontDef.FontSize, fontDef.FontStyle);

    public Font LoadFont(string fontName, float size, FontStyle style)
    {
        return LoadFontPreinstalled(fontName, size, style) ?? 
            LoadFontCustom(fontName, size, style) ?? 
            throw new FileNotFoundException($"Font '{fontName}' not found as preinstalled or custom font.");
    }

    private static Font? LoadFontPreinstalled(string fontName, float size, FontStyle style)
    {
        var font = new Font(fontName, size, (System.Drawing.FontStyle)style, GraphicsUnit.Pixel);

        if (font.Name == fontName)
            return font;
        else
            return null;
    }

    private static Font? LoadFontCustom(string fontPath, float size, FontStyle style)
    {
        if (!Files.FileExists(fontPath))
            fontPath = DEFAULT_FONT_FOLDER + fontPath;

        if (!Files.FileExists(fontPath))
            fontPath = fontPath + ".ttf";

        if (!Files.FileExists(fontPath))
            return null;

        using var pfc = new PrivateFontCollection();

        pfc.AddFontFile(fontPath);

        return new Font(pfc.Families[0], size, (System.Drawing.FontStyle)style, GraphicsUnit.Pixel);
    }
}
