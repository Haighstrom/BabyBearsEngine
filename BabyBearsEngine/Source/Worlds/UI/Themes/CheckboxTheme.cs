using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="Checkbox"/>: the box itself reuses a full
/// <see cref="ButtonTheme"/> (so it gets idle / hover / pressed / disabled tints), and the
/// tick overlay is produced by a separate factory.
/// </summary>
/// <remarks>
/// <para>Nesting <see cref="ButtonTheme"/> inside <see cref="CheckboxTheme"/> is deliberate
/// — a checkbox <em>is</em> a button with an extra overlay, and reusing the existing button
/// styling avoids re-stating the four state tints. There is no ambiguity here because the
/// engine ships no top-level theme bundle, so <see cref="Box"/> is the unique owner of the
/// box's appearance.</para>
/// <para><see cref="TickFactory"/> is invoked once per checkbox with the checkbox's local
/// rectangle. The default factories produce a simple filled inset square as the tick —
/// unambiguous as a "filled state" indicator without needing a glyph or texture.</para>
/// </remarks>
public sealed record CheckboxTheme
{
    private static readonly Colour s_defaultBoxColour = new(180, 180, 180);
    private static readonly Colour s_defaultTickColour = Colour.Black;

    /// <summary>Styling for the box behind the tick — idle / hover / pressed / disabled tints and the box's background factory.</summary>
    public required ButtonTheme Box { get; init; }

    /// <summary>
    /// Factory producing the tick overlay for one checkbox. Called once per checkbox with
    /// the checkbox's local rectangle (origin <c>(0, 0)</c>, the checkbox's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> TickFactory { get; init; }

    /// <summary>Bland placeholder theme — grey box with a black inset square as the tick. Prototyping only.</summary>
    public static readonly CheckboxTheme Default = FromColours(s_defaultBoxColour, s_defaultTickColour);

    /// <summary>
    /// Builds a theme with a solid-colour box (via <see cref="ButtonTheme.FromColour"/>) and
    /// a solid-colour tick rendered as a filled square inset by 20% of the checkbox size.
    /// </summary>
    public static CheckboxTheme FromColours(Colour boxColour, Colour tickColour) => new()
    {
        Box = ButtonTheme.FromColour(boxColour),
        TickFactory = r => new ColourGraphic(
            tickColour,
            r.X + r.W * 0.2f,
            r.Y + r.H * 0.2f,
            r.W * 0.6f,
            r.H * 0.6f),
    };

    /// <summary>
    /// Builds a theme with a textured box and a textured tick. Both textures are stretched
    /// to the checkbox's full local rectangle.
    /// </summary>
    public static CheckboxTheme FromTextures(ITexture boxTexture, ITexture tickTexture) => new()
    {
        Box = ButtonTheme.FromTexture(boxTexture),
        TickFactory = r => new TextureGraphic(tickTexture, r.X, r.Y, r.W, r.H),
    };
}
