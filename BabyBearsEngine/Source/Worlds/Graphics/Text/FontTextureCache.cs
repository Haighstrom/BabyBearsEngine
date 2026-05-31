namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Memoises <see cref="FontAtlas"/> instances by <see cref="FontDefinition"/>, so each
/// font is generated and uploaded to the GPU only once across the lifetime of the
/// application. The backend is resolved per font — the definition's
/// <see cref="FontDefinition.Renderer"/> if set, otherwise
/// <see cref="EngineConfiguration.DefaultTextRenderer"/>; this class only handles caching.
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

        // A font may pin its own backend; otherwise it follows the engine-wide default.
        TextRenderer renderer = fontDefinition.Renderer ?? EngineConfiguration.DefaultTextRenderer;

        // Fail fast at the moment a TextGraphic (or any cache caller) asks for an atlas, rather
        // than deep inside the GDI rasteriser's Generate(). The configure-time setter on
        // EngineConfiguration.DefaultTextRenderer catches the engine-default case earlier still;
        // this branch additionally covers FontDefinition.Renderer per-font pins.
        if (renderer == TextRenderer.Gdi && !OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                $"{nameof(TextRenderer)}.{nameof(TextRenderer.Gdi)} requires Windows; use {nameof(TextRenderer)}.{nameof(TextRenderer.Sdf)} or {nameof(TextRenderer)}.{nameof(TextRenderer.FreeType)} on other platforms.");
        }

        FontAtlas atlas = EngineConfiguration.GetAtlasGenerator(renderer).Generate(fontDefinition);
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
