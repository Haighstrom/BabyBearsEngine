using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="ProgressBar"/>: two factories, one for the background
/// (the empty part of the bar) and one for the fill (the proportion that grows with
/// <see cref="ProgressBar.AmountFilled"/>).
/// </summary>
public sealed record ProgressBarTheme
{
    private static readonly Colour s_defaultBackground = new(60, 60, 60);
    private static readonly Colour s_defaultFill = new(80, 200, 80);

    /// <summary>
    /// Factory producing the background ("empty") graphic for one bar. Called once with
    /// the bar's full local rectangle (origin <c>(0, 0)</c>, the bar's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> BackgroundFactory { get; init; }

    /// <summary>
    /// Factory producing the fill graphic for one bar. Called once with the bar's full
    /// local rectangle; the <see cref="ProgressBar"/> mutates the returned graphic's
    /// <see cref="IGraphic.Width"/> to track <see cref="ProgressBar.AmountFilled"/>.
    /// </summary>
    public required Func<Rect, IGraphic> FillFactory { get; init; }

    /// <summary>Bland placeholder theme — dark grey background, green fill. Prototyping only.</summary>
    public static readonly ProgressBarTheme Default = FromColours(s_defaultBackground, s_defaultFill);

    /// <summary>Builds a theme with solid-colour background and fill rectangles.</summary>
    public static ProgressBarTheme FromColours(Colour background, Colour fill) => new()
    {
        BackgroundFactory = r => new ColouredRectangle(background, r.X, r.Y, r.W, r.H),
        FillFactory = r => new ColouredRectangle(fill, r.X, r.Y, r.W, r.H),
    };

    /// <summary>Builds a theme with textured background and fill.</summary>
    public static ProgressBarTheme FromTextures(ITexture background, ITexture fill) => new()
    {
        BackgroundFactory = r => new Image(background, r.X, r.Y, r.W, r.H),
        FillFactory = r => new Image(fill, r.X, r.Y, r.W, r.H),
    };
}
