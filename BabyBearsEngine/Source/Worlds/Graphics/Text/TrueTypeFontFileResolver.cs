using System.IO;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Resolves a <see cref="FontDefinition.FontName"/> to a TrueType font file on disk for the
/// text backends that rasterise raw glyph outlines (SDF and FreeType). Unlike the GDI+ backend —
/// which renders installed system fonts by family name — these backends need the font shipped as
/// a <c>.ttf</c> file. The convention is that the file name (without extension) matches the
/// FontName and lives in <c>Assets/Fonts/</c>; a full or relative path is also accepted.
/// </summary>
internal static class TrueTypeFontFileResolver
{
    private const string DefaultFontFolder = "Assets/Fonts/";

    public static string Resolve(string fontName)
    {
        foreach (string candidate in CandidatePaths(fontName))
        {
            if (Files.FileExists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            $"The SDF and FreeType text backends require a TrueType font file for '{fontName}'. " +
            $"Place a file named '{fontName}.ttf' in '{DefaultFontFolder}' (the file name must match the FontName), " +
            $"or pass a valid file path as the FontName.");
    }

    private static string[] CandidatePaths(string fontName) =>
    [
        fontName,
        fontName + ".ttf",
        DefaultFontFolder + fontName,
        DefaultFontFolder + fontName + ".ttf",
    ];
}
