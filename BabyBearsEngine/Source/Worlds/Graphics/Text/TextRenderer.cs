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
    /// </summary>
    Gdi,
}
