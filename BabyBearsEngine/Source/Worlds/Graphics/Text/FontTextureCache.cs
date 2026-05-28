using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;

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

    private static CachedFontAtlas Build(FontDefinition fontDefinition)
    {
        var font = new FontLoader().LoadFont(fontDefinition);
        GeneratedFontStruct fontStruct = new FontBitmapGenerator().GenerateCharSpritesheetAndPositions(
            font, fontDefinition.CharactersToLoad, fontDefinition.AntiAliased, 13);
        ITexture texture = new DefaultTextureFactory().GenTexture(fontStruct.CharacterSS);
        // One shader instance per cached atlas. With the previous design every TextGraphic
        // newed its own StandardMatrixShaderProgram, which meant a fresh GL program compile
        // per text object. Sharing per font is both correct (matrices are set per Render
        // call) and cheaper.
        IMatrixShaderProgram shader = new StandardMatrixShaderProgram();
        return new CachedFontAtlas(fontStruct, texture, shader);
    }
}
