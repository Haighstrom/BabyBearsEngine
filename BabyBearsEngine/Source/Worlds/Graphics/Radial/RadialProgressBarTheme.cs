using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Visual styling for a <see cref="RadialProgressBar"/>: two factories, one for the background
/// (the empty track) and one for the fill (the sector/annulus that grows with
/// <see cref="RadialProgressBar.AmountFilled"/>) — mirroring <see cref="ProgressBarTheme"/>.
/// The radial-specific parameters (fill style, ring thickness, start angle, sweep direction,
/// arc quality) are baked into the fill factory by the static builder methods below.
/// </summary>
public sealed record RadialProgressBarTheme
{
    private static readonly Colour s_defaultBackground = new(60, 60, 60);
    private static readonly Colour s_defaultFill = new(80, 200, 80);

    /// <summary>
    /// Factory producing the background ("empty track") graphic. Called once with the bar's
    /// full local rectangle (origin <c>(0, 0)</c>, the bar's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> BackgroundFactory { get; init; }

    /// <summary>
    /// Factory producing the fill graphic. Called once with the bar's full local rectangle.
    /// <see cref="RadialProgressBar"/> tracks <see cref="RadialProgressBar.AmountFilled"/> by
    /// setting <c>AmountFilled</c> on a <see cref="RadialFillGraphic"/> or
    /// <see cref="RadialTextureFillGraphic"/> fill.
    /// </summary>
    public required Func<Rect, IGraphic> FillFactory { get; init; }

    /// <summary>Bland placeholder theme — dark grey background, green pie fill. Prototyping only.</summary>
    public static readonly RadialProgressBarTheme Default = FromColours(s_defaultBackground, s_defaultFill);

    /// <summary>Builds a theme with a solid-colour square background and a solid-colour radial fill.</summary>
    /// <param name="background">Background fill colour.</param>
    /// <param name="fill">Radial fill colour.</param>
    /// <param name="fillStyle">Pie or ring. Defaults to <see cref="RadialFillStyle.Pie"/>.</param>
    /// <param name="ringThickness">Ring band thickness as a fraction of the outer radius, in (0, 1]. Ignored for <see cref="RadialFillStyle.Pie"/>. Defaults to 0.3.</param>
    /// <param name="startAngleDegrees">Sweep start angle (clock convention — see <see cref="RadialFillMeshBuilder"/>). Defaults to 0 (12 o'clock).</param>
    /// <param name="direction">Sweep direction from the start angle. Defaults to <see cref="RadialSweepDirection.Clockwise"/>.</param>
    /// <param name="segments">Arc segments for a full 0..1 sweep; higher is smoother. Defaults to 64.</param>
    public static RadialProgressBarTheme FromColours(Colour background, Colour fill, RadialFillStyle fillStyle = RadialFillStyle.Pie, float ringThickness = 0.3f, float startAngleDegrees = 0f, RadialSweepDirection direction = RadialSweepDirection.Clockwise, int segments = 64) => new()
    {
        BackgroundFactory = r => new ColourGraphic(background, r.X, r.Y, r.W, r.H),
        FillFactory = r => new RadialFillGraphic(fill, r, fillStyle, ringThickness, startAngleDegrees, direction, segments),
    };

    /// <summary>Builds a theme with a textured square background and a polar-mapped textured radial fill.</summary>
    /// <param name="background">Background texture.</param>
    /// <param name="fill">Radial fill texture, polar-mapped (see <see cref="RadialTextureFillGraphic"/>).</param>
    /// <param name="fillStyle">Pie or ring. Defaults to <see cref="RadialFillStyle.Pie"/>.</param>
    /// <param name="ringThickness">Ring band thickness as a fraction of the outer radius, in (0, 1]. Ignored for <see cref="RadialFillStyle.Pie"/>. Defaults to 0.3.</param>
    /// <param name="startAngleDegrees">Sweep start angle (clock convention — see <see cref="RadialFillMeshBuilder"/>). Defaults to 0 (12 o'clock).</param>
    /// <param name="direction">Sweep direction from the start angle. Defaults to <see cref="RadialSweepDirection.Clockwise"/>.</param>
    /// <param name="segments">Arc segments for a full 0..1 sweep; higher is smoother. Defaults to 64.</param>
    public static RadialProgressBarTheme FromTextures(ITexture background, ITexture fill, RadialFillStyle fillStyle = RadialFillStyle.Pie, float ringThickness = 0.3f, float startAngleDegrees = 0f, RadialSweepDirection direction = RadialSweepDirection.Clockwise, int segments = 64) => new()
    {
        BackgroundFactory = r => new TextureGraphic(background, r.X, r.Y, r.W, r.H),
        FillFactory = r => new RadialTextureFillGraphic(fill, r, fillStyle, ringThickness, startAngleDegrees, direction, segments),
    };

    /// <summary>Builds a theme with background and fill textures both loaded from path strings. See <see cref="FromTextures"/> for parameter details.</summary>
    public static RadialProgressBarTheme FromTexturePaths(string backgroundPath, string fillPath, RadialFillStyle fillStyle = RadialFillStyle.Pie, float ringThickness = 0.3f, float startAngleDegrees = 0f, RadialSweepDirection direction = RadialSweepDirection.Clockwise, int segments = 64) =>
        FromTextures(Textures.CreateFromFile(backgroundPath), Textures.CreateFromFile(fillPath), fillStyle, ringThickness, startAngleDegrees, direction, segments);
}
