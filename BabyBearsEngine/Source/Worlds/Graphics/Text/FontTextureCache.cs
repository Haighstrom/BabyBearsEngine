namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Memoises <see cref="FontAtlas"/> instances by <see cref="FontDefinition"/>, so each
/// font is generated and uploaded to the GPU only once across the lifetime of the
/// application. The atlas generator itself is configured via
/// <see cref="EngineConfiguration.AtlasGenerator"/>; this class only handles caching.
/// </summary>
internal static class FontTextureCache
{
    private static readonly Dictionary<FontDefinition, FontAtlas> s_cache = [];

    internal static FontAtlas GetOrCreate(FontDefinition fontDefinition)
    {
        if (s_cache.TryGetValue(fontDefinition, out var existing))
        {
            return existing;
        }

        FontAtlas atlas = EngineConfiguration.AtlasGenerator.Generate(fontDefinition);
        s_cache[fontDefinition] = atlas;
        return atlas;
    }

    /// <summary>
    /// Drops all cached atlases. Called by <see cref="EngineConfiguration"/> when the
    /// generator changes or the engine is reset — cached entries reference the old
    /// generator's GL texture and shader, so they cannot survive a backend swap.
    /// </summary>
    internal static void InvalidateCache() => s_cache.Clear();
}
