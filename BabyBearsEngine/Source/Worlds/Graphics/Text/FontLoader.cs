using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Worlds.Graphics.Text;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
internal class FontLoader() : IFontLoader
{
    private const string DEFAULT_FONT_FOLDER = "Assets/Fonts/";

    // Custom fonts loaded via PrivateFontCollection are referenced by the returned Font via a
    // FontFamily handle owned by the collection. Disposing the collection at the end of the
    // load method (as a `using` would) invalidates that handle and the Font silently breaks —
    // GDI+ documents this as undefined. Cache one collection per path for the lifetime of the
    // process so the family handles stay live and the same font isn't re-loaded from disk on
    // repeat requests.
    private static readonly Lock s_customCacheLock = new();
    private static readonly Dictionary<string, PrivateFontCollection> s_customCache = new(StringComparer.OrdinalIgnoreCase);

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
        {
            return font;
        }
        else
        {
            return null;
        }
    }

    private static Font? LoadFontCustom(string fontPath, float size, FontStyle style)
    {
        if (!Files.FileExists(fontPath))
        {
            fontPath = DEFAULT_FONT_FOLDER + fontPath;
        }

        if (!Files.FileExists(fontPath))
        {
            fontPath = fontPath + ".ttf";
        }

        if (!Files.FileExists(fontPath))
        {
            return null;
        }

        PrivateFontCollection pfc;
        lock (s_customCacheLock)
        {
            if (!s_customCache.TryGetValue(fontPath, out PrivateFontCollection? cached))
            {
                cached = new PrivateFontCollection();
                cached.AddFontFile(fontPath);
                s_customCache[fontPath] = cached;
            }
            pfc = cached;
        }

        return new Font(pfc.Families[0], size, (System.Drawing.FontStyle)style, GraphicsUnit.Pixel);
    }
}
