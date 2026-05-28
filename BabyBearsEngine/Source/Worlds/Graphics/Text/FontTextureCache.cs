namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Memoises <see cref="FontAtlas"/> instances by <see cref="FontDefinition"/>, so each
/// font is generated and uploaded to the GPU only once across the lifetime of the
/// application. Atlas generation itself is delegated to <see cref="Generator"/>.
/// </summary>
internal static class FontTextureCache
{
    private static readonly Dictionary<FontDefinition, FontAtlas> s_cache = [];
    private static IFontAtlasGenerator s_generator = new GdiFontAtlasGenerator();

    /// <summary>
    /// The atlas generator used to build new <see cref="FontAtlas"/>es. Defaults to
    /// <see cref="GdiFontAtlasGenerator"/>. Set this at startup — before any text is
    /// rendered — to switch the atlas backend (for example to an SDF-based generator).
    /// Assigning a new generator invalidates any cached atlases, because cached entries
    /// are tied to the previous generator's texture format and paired shader.
    /// </summary>
    internal static IFontAtlasGenerator Generator
    {
        get => s_generator;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            s_generator = value;
            s_cache.Clear();
        }
    }

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

    /// <summary>
    /// Restore the default generator and clear the cache. A test seam — production
    /// code should not call this.
    /// </summary>
    internal static void Reset()
    {
        s_generator = new GdiFontAtlasGenerator();
        s_cache.Clear();
    }
}
