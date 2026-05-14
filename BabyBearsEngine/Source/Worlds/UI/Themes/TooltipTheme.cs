using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="SimpleToolTip"/>: the background graphic factory and the
/// text style of the tooltip's label.
/// </summary>
public sealed record TooltipTheme
{
    private static readonly Colour s_defaultBackground = new(60, 60, 60, 230);

    /// <summary>
    /// Factory producing the tooltip's background graphic. Called once with the tooltip's
    /// local rectangle (origin <c>(0, 0)</c>, the tooltip's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> BackgroundFactory { get; init; }

    /// <summary>Styling for the tooltip's label text.</summary>
    public required TextTheme Text { get; init; }

    /// <summary>Bland placeholder theme — translucent dark grey background, white text. Prototyping only.</summary>
    public static readonly TooltipTheme Default = FromColours(s_defaultBackground, Colour.White);

    /// <summary>Builds a theme with a solid-colour background and a custom text colour.</summary>
    public static TooltipTheme FromColours(Colour background, Colour textColour) => new()
    {
        BackgroundFactory = r => new ColouredRectangle(background, r.X, r.Y, r.W, r.H),
        Text = TextTheme.Default with { Colour = textColour },
    };

    /// <summary>Builds a theme with a textured background and a custom text colour.</summary>
    public static TooltipTheme FromTexture(ITexture background, Colour textColour) => new()
    {
        BackgroundFactory = r => new Image(background, r.X, r.Y, r.W, r.H),
        Text = TextTheme.Default with { Colour = textColour },
    };
}
