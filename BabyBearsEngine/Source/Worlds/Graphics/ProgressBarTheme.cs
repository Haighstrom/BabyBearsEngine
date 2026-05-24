using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

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
    /// local rectangle. The <see cref="ProgressBar"/> tracks <see cref="ProgressBar.AmountFilled"/>
    /// by setting the <see cref="TextureGraphic.SourceArea"/> of a <see cref="TextureGraphic"/>
    /// fill (so the texture is clipped, not stretched), otherwise by mutating the fill's
    /// <see cref="IGraphic.Width"/>.
    /// </summary>
    public required Func<Rect, IGraphic> FillFactory { get; init; }

    /// <summary>Bland placeholder theme — dark grey background, green fill. Prototyping only.</summary>
    public static readonly ProgressBarTheme Default = FromColours(s_defaultBackground, s_defaultFill);

    /// <summary>Builds a theme with solid-colour background and fill rectangles.</summary>
    public static ProgressBarTheme FromColours(Colour background, Colour fill) => new()
    {
        BackgroundFactory = r => new ColourGraphic(background, r.X, r.Y, r.W, r.H),
        FillFactory = r => new ColourGraphic(fill, r.X, r.Y, r.W, r.H),
    };

    /// <summary>Builds a theme with solid-colour background and fill rectangles, each drawn with a border.</summary>
    public static ProgressBarTheme FromBorderedColours(Colour background, Colour fill, Colour border, float borderThickness) => new()
    {
        BackgroundFactory = r => new BorderedColourGraphic(r, background, border, borderThickness),
        FillFactory = r => new BorderedColourGraphic(r, fill, border, borderThickness),
    };

    /// <summary>Builds a theme with a transparent background and a fill loaded from a path string.</summary>
    public static ProgressBarTheme FromFillTexturePath(string fillPath) => new()
    {
        BackgroundFactory = r => new ColourGraphic(new Colour(0, 0, 0, 0), r.X, r.Y, r.W, r.H),
        FillFactory = r => new TextureGraphic(Textures.CreateFromFile(fillPath), r.X, r.Y, r.W, r.H),
    };

    /// <summary>Builds a theme with background and fill both loaded from path strings.</summary>
    public static ProgressBarTheme FromTexturePaths(string backgroundPath, string fillPath) => new()
    {
        BackgroundFactory = r => new TextureGraphic(Textures.CreateFromFile(backgroundPath), r.X, r.Y, r.W, r.H),
        FillFactory = r => new TextureGraphic(Textures.CreateFromFile(fillPath), r.X, r.Y, r.W, r.H),
    };

    /// <summary>Builds a theme with textured background and fill.</summary>
    public static ProgressBarTheme FromTextures(ITexture background, ITexture fill) => new()
    {
        BackgroundFactory = r => new TextureGraphic(background, r.X, r.Y, r.W, r.H),
        FillFactory = r => new TextureGraphic(fill, r.X, r.Y, r.W, r.H),
    };
}
