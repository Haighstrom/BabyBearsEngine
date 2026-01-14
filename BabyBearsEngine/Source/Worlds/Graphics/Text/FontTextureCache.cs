using System.Collections.Generic;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

internal class FontTextureCache
{
    private static readonly Dictionary<FontDefinition, FontTexture> s_fontTextures = [];

    internal static FontTexture GetOrCreate(FontDefinition fontDefinition)
    {
        if (s_fontTextures.TryGetValue(fontDefinition, out var existingFontAtlas))
        {
            return existingFontAtlas;
        }

        var newFontAtlas = BuildFontTexture(fontDefinition);

        s_fontTextures[fontDefinition] = newFontAtlas;

        return newFontAtlas;
    }

    private static FontTexture BuildFontTexture(FontDefinition fontDefinition)
    {
        // Your sprite sheet build logic here
        throw new NotImplementedException();
    }
}
