namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Selects which text rendering backend the engine uses to build font atlases.
/// Set on <see cref="TextSettings.Renderer"/>; resolved at startup by
/// <see cref="GameLauncher"/> into a concrete <c>IFontAtlasGenerator</c>.
/// </summary>
public enum TextRenderer
{
    /// <summary>
    /// GDI+ bitmap rasterisation. Glyphs are rendered at the font's chosen pixel
    /// size into a coverage (alpha) bitmap atlas. Crisp at the source size; quality
    /// degrades when text is scaled because the texture's resolution is fixed.
    /// Windows-only.
    /// </summary>
    Gdi,

    /// <summary>
    /// Signed distance field rasterisation via stb_truetype. Glyphs are stored as
    /// distance-to-outline rather than coverage, so text stays crisp at any scale.
    /// Cross-platform, but requires the font shipped as a <c>.ttf</c> file in
    /// <c>Assets/Fonts/</c> named to match the <see cref="FontDefinition.FontName"/>.
    /// </summary>
    Sdf,
}
