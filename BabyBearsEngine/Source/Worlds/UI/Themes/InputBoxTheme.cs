using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="TextInputBox"/> or <see cref="NumberInputBox"/>: the
/// background, text style, cursor colour, and selection highlight colour.
/// </summary>
/// <remarks>
/// Construct via <see cref="FromColour"/> for a solid-colour background, or supply a custom
/// <see cref="BackgroundFactory"/> with a <c>with</c>-expression. <see cref="Default"/> is
/// intentionally bland — light-grey on white — so prototype UI looks like prototype UI.
/// </remarks>
public sealed record InputBoxTheme
{
    private static readonly Colour s_defaultBackground = new(250, 250, 250);

    /// <summary>
    /// Factory producing the background graphic for one input box. Called once per instance
    /// with the box's local rectangle (origin <c>(0,0)</c>, the box's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> BackgroundFactory { get; init; }

    /// <summary>Styling for the text displayed inside the box.</summary>
    public required TextTheme Text { get; init; }

    /// <summary>Colour of the blinking cursor line.</summary>
    public required Colour CursorColour { get; init; }

    /// <summary>Colour of the selection highlight rectangle.</summary>
    public required Colour SelectionColour { get; init; }

    /// <summary>
    /// Bland placeholder theme — near-white background, black cursor, light-blue selection,
    /// left-aligned <see cref="TextTheme.Default"/> text. For prototyping only.
    /// </summary>
    public static readonly InputBoxTheme Default = FromColour(s_defaultBackground);

    /// <summary>
    /// Builds a theme whose background is a solid <see cref="ColourGraphic"/> in
    /// <paramref name="backgroundColour"/>, with a black cursor and a translucent blue
    /// selection highlight.
    /// </summary>
    public static InputBoxTheme FromColour(Colour backgroundColour)
    {
        return new()
        {
            BackgroundFactory = r => new ColourGraphic(backgroundColour, r.X, r.Y, r.W, r.H),
            Text = TextTheme.Default with { HAlignment = HAlignment.Left, VAlignment = VAlignment.Centred },
            CursorColour = Colour.Black,
            SelectionColour = new Colour(100, 160, 255, 100),
        };
    }
}
