namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Memoises <see cref="FontAtlas"/> instances by <see cref="FontDefinition"/>, so each
/// font is generated and uploaded to the GPU only once across the lifetime of the
/// application. Atlas generation itself is delegated to <see cref="s_generator"/>.
/// </summary>
internal static class FontTextureCache
{
    // Hardcoded to the GDI+ generator for now. PR 4 will make this swappable so a
    // game can opt into an SDF or MSDF generator at startup with a single line.
    private static readonly IFontAtlasGenerator s_generator = new GdiFontAtlasGenerator();

    private static readonly Dictionary<FontDefinition, FontAtlas> s_cache = [];

    internal static FontAtlas GetOrCreate(FontDefinition fontDefinition)
    {
        if (s_cache.TryGetValue(fontDefinition, out var existing))
        {
            return existing;
        }

        FontAtlas atlas = s_generator.Generate(fontDefinition);
        s_cache[fontDefinition] = atlas;
        return atlas;
    }
}
