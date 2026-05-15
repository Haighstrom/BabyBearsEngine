using System.Collections.Generic;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal static class FontTextureCache
{
    private static readonly Dictionary<FontDefinition, CachedFontAtlas> s_cache = [];

    internal static CachedFontAtlas GetOrCreate(FontDefinition fontDefinition)
    {
        if (s_cache.TryGetValue(fontDefinition, out var existing))
        {
            return existing;
        }

        CachedFontAtlas atlas = Build(fontDefinition);
        s_cache[fontDefinition] = atlas;
        return atlas;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "GDI+ only called on Windows.")]
    private static CachedFontAtlas Build(FontDefinition fontDefinition)
    {
        var font = new FontLoader().LoadFont(fontDefinition);
        GeneratedFontStruct fontStruct = new FontBitmapGenerator().GenerateCharSpritesheetAndPositions(
            font, fontDefinition.CharactersToLoad, fontDefinition.AntiAliased, 13);
        ITexture texture = new DefaultTextureFactory().GenTexture(fontStruct.CharacterSS);
        return new CachedFontAtlas(fontStruct, texture);
    }
}
