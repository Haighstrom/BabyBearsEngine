using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="Button"/>: the tint colour for each interaction state
/// (default / hover / pressed / disabled), the text style for the button's label, and a
/// factory that produces the button's background graphic on demand.
/// </summary>
/// <remarks>
/// <para>The state colours are <em>tints</em> applied to whatever <see cref="BackgroundFactory"/>
/// produces. For a solid-colour theme this means the rectangle's fill colour changes between
/// states; for a textured theme the texture is multiplied by the state colour (with
/// <see cref="Colour.White"/> meaning "untinted").</para>
/// <para><see cref="BackgroundFactory"/> is invoked once per <see cref="Button"/>, with the
/// button's local rectangle, so each button gets its own background instance — themes are
/// freely reusable across many widgets.</para>
/// <para>Construct via <see cref="FromColour"/>, <see cref="FromTexture"/>, or
/// <see cref="FromGraphic"/> for common cases; use the record's <c>with</c>-expression to
/// vary a single field. <see cref="Default"/> is deliberately bland — plain grey — so
/// prototype UI looks like prototype UI.</para>
/// </remarks>
public sealed record ButtonTheme
{
    private static readonly Colour s_defaultBaseColour = new(180, 180, 180);

    /// <summary>Tint applied to the background when the button is idle (no hover, press, or disabled state).</summary>
    public required Colour Idle { get; init; }

    /// <summary>Tint applied while the cursor is hovering over the button.</summary>
    public required Colour Hover { get; init; }

    /// <summary>Tint applied while the left mouse button is held down on the button.</summary>
    public required Colour Pressed { get; init; }

    /// <summary>Tint applied when <see cref="Button.Disabled"/> is <c>true</c>.</summary>
    public required Colour Disabled { get; init; }

    /// <summary>Styling for the button's label text.</summary>
    public required TextTheme Text { get; init; }

    /// <summary>
    /// Factory producing the background graphic for one button. Called once per button with
    /// the button's local rectangle (origin <c>(0, 0)</c>, the button's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> BackgroundFactory { get; init; }

    /// <summary>
    /// Bland placeholder theme — mid-grey background, slightly darker on hover / pressed,
    /// pale on disabled, with <see cref="TextTheme.Default"/> for the label. For prototyping
    /// only; build a real <see cref="ButtonTheme"/> before shipping.
    /// </summary>
    public static readonly ButtonTheme Default = FromColour(s_defaultBaseColour);

    /// <summary>
    /// Builds a theme whose background is a solid <see cref="ColourGraphic"/> in
    /// <paramref name="baseColour"/>, with hover and pressed tints synthesised in whichever
    /// direction has headroom — light base colours darken on hover, dark ones lighten — and
    /// a disabled tint produced by fading the base colour's alpha.
    /// </summary>
    public static ButtonTheme FromColour(Colour baseColour)
    {
        bool isLight = baseColour.R + baseColour.G + baseColour.B > 384;
        Colour hover = isLight ? baseColour.Darkened(0.05f) : baseColour.Lightened(0.05f);
        Colour pressed = isLight ? baseColour.Darkened(0.1f) : baseColour.Lightened(0.1f);

        return new()
        {
            Idle = baseColour,
            Hover = hover,
            Pressed = pressed,
            Disabled = new Colour(baseColour, 0.4f),
            Text = TextTheme.Default,
            BackgroundFactory = r => new ColourGraphic(baseColour, r.X, r.Y, r.W, r.H),
        };
    }

    /// <summary>
    /// Builds a theme whose background is an <see cref="TextureGraphic"/> sampling
    /// <paramref name="texture"/>, with hover and pressed states applied as tint colours over
    /// the texture sample.
    /// </summary>
    public static ButtonTheme FromTexture(ITexture texture) => new()
    {
        Idle = Colour.White,
        Hover = new Colour(230, 230, 230),
        Pressed = new Colour(200, 200, 200),
        Disabled = new Colour(255, 255, 255, 128),
        Text = TextTheme.Default,
        BackgroundFactory = r => new TextureGraphic(texture, r.X, r.Y, r.W, r.H),
    };

    /// <summary>
    /// Builds a theme using a caller-supplied background factory. The factory must produce a
    /// fresh <see cref="IGraphic"/> per call (it's invoked once per button). State
    /// tints default to white-based variants — override via <c>with</c> if the supplied
    /// graphic needs different multipliers.
    /// </summary>
    public static ButtonTheme FromGraphic(Func<Rect, IGraphic> backgroundFactory) => new()
    {
        Idle = Colour.White,
        Hover = new Colour(230, 230, 230),
        Pressed = new Colour(200, 200, 200),
        Disabled = new Colour(255, 255, 255, 128),
        Text = TextTheme.Default,
        BackgroundFactory = backgroundFactory,
    };
}
