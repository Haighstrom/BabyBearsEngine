using BabyBearsEngine.Rendering.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a piece of text rendered by a UI widget — the font to draw with, the
/// colour to draw it in, and how to align it inside the widget's local rectangle.
/// </summary>
/// <remarks>
/// Themes are deliberately a small, immutable bundle: clone with <c>with</c>-expressions
/// (<c>existing with { Colour = Colour.Red }</c>) to compose variations.
/// <para><see cref="Default"/> is intentionally bland (plain Times New Roman, black, centred)
/// so prototype UI looks like prototype UI and isn't mistaken for finished styling.</para>
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
    /// prototyping; replace with a real <see cref="TextTheme"/> before shipping.
    /// </summary>
    public static readonly TextTheme Default = new(
        new FontDefinition("Times New Roman", 16),
        Colour.Black);
}
