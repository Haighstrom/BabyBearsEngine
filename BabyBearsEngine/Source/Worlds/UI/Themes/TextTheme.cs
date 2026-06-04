using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a piece of text rendered by a UI widget — the font to draw with, the
/// colour to draw it in, and how to align it inside the widget's local rectangle.
/// </summary>
/// <remarks>
/// Themes are deliberately a small, immutable bundle: clone with <c>with</c>-expressions
/// (<c>existing with { Colour = Colour.Red }</c>) to compose variations.
/// <para><see cref="Default"/> is intentionally bland (plain Times New Roman, black, centred)
/// so prototype UI looks like prototype UI and isn't mistaken for finished styling. It is also
/// <em>settable</em> so the host app can swap a cross-platform font in once at startup and have
/// every downstream <c>*.Default</c> pick it up (Times New Roman ships with Windows but not
/// stock Linux/macOS, so games targeting those platforms should set <see cref="Default"/>
/// before any UI is constructed).</para>
/// </remarks>
/// <param name="Font">Font definition used to render the text.</param>
/// <param name="Colour">Colour to draw the glyphs in.</param>
/// <param name="HAlignment">Horizontal alignment within the widget's local rectangle.</param>
/// <param name="VAlignment">Vertical alignment within the widget's local rectangle.</param>
public sealed record TextTheme(
    FontDefinition Font,
    Colour Colour,
    HAlignment HAlignment = HAlignment.Centred,
    VAlignment VAlignment = VAlignment.Centred)
{
    /// <summary>
    /// Plain placeholder text style — Times New Roman 16pt, black, centred. Use during
    /// prototyping; replace before shipping. Settable so the host app can swap in a
    /// cross-platform font once at startup; downstream <c>*.Default</c> themes that clone this
    /// will then inherit the swap.
    /// </summary>
    public static TextTheme Default { get; set; } = new(
        new FontDefinition("Times New Roman", 16),
        Colour.Black);
}
