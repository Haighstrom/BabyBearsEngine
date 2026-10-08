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

    // Atlases rasterised at the on-screen pixel size rather than the canvas size (see TextRenderScale),
    // keyed by the base definition. Dropped whenever the scale changes.
    private static readonly Dictionary<FontDefinition, FontAtlas> s_scaledCache = [];

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
    /// Returns the font's coverage atlas rasterised at <paramref name="scale"/> times its size, for
    /// drawing 1:1 on screen when the canvas is stretched. Only meaningful for backends that author
    /// glyphs at the requested pixel size; the caller decides whether that applies.
    /// </summary>
    internal static FontAtlas GetOrCreateScaled(FontDefinition fontDefinition, float scale)
    {
        // The scale is part of the key so a stale entry can never be returned for a new scale.
        FontDefinition scaledDefinition = fontDefinition with { FontSize = MathF.Max(1f, MathF.Round(fontDefinition.FontSize * scale)) };

        if (s_scaledCache.TryGetValue(scaledDefinition, out FontAtlas? existing))
        {
            return existing;
        }

        TextRenderer renderer = scaledDefinition.Renderer ?? EngineConfiguration.DefaultTextRenderer;
        FontAtlas atlas = EngineConfiguration.GetAtlasGenerator(renderer).Generate(scaledDefinition);
        s_scaledCache[scaledDefinition] = atlas;
        return atlas;
    }

    /// <summary>Disposes and drops every scaled atlas; called when the render scale changes.</summary>
    internal static void InvalidateScaled()
    {
        foreach (FontAtlas atlas in s_scaledCache.Values)
        {
            atlas.Texture.Dispose();
        }

        s_scaledCache.Clear();
    }

    /// <summary>
    /// Drops all cached atlases. Called by <see cref="EngineConfiguration"/> when the
    /// generator changes or the engine is reset — cached entries reference the old
    /// generator's GL texture, so they cannot survive a backend swap.
    /// </summary>
    /// <remarks>
    /// Each atlas owns its own <see cref="OpenGL.ITexture"/> (uploaded per font), so the
    /// texture is disposed before the cache entry is dropped — otherwise the GL handle would
    /// leak on every backend swap. The atlas's shader is a process-wide singleton shared with
    /// non-text graphics (Shaders.SdfText / CoverageText / Standard) and is NOT disposed here.
    /// </remarks>
    internal static void InvalidateCache()
    {
        foreach (FontAtlas atlas in s_cache.Values)
        {
            atlas.Texture.Dispose();
        }
        s_cache.Clear();
        InvalidateScaled();
    }
}
