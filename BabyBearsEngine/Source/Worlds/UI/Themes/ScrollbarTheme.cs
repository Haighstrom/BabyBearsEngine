using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="Scrollbar"/>: a factory for the track (the static
/// background), and a <see cref="ButtonTheme"/> for the thumb (so it gets idle / hover /
/// pressed / disabled tints for free — the thumb is implemented as a <see cref="Button"/>
/// internally).
/// </summary>
public sealed record ScrollbarTheme
{
    private static readonly Colour s_defaultTrack = new(60, 60, 60);
    private static readonly Colour s_defaultThumb = new(160, 160, 160);

    /// <summary>
    /// Factory producing the track graphic — the static background behind the thumb.
    /// Called once with the scrollbar's full local rectangle.
    /// </summary>
    public required Func<Rect, IGraphic> TrackFactory { get; init; }

    /// <summary>Styling for the draggable thumb. Reusing <see cref="ButtonTheme"/> gives it hover and pressed tints.</summary>
    public required ButtonTheme Thumb { get; init; }

    /// <summary>Bland placeholder theme — dark grey track, mid-grey thumb. Prototyping only.</summary>
    public static readonly ScrollbarTheme Default = FromColours(s_defaultTrack, s_defaultThumb);

    /// <summary>Builds a theme with a solid-colour track and a solid-colour thumb (with synthesised hover / pressed tints).</summary>
    public static ScrollbarTheme FromColours(Colour track, Colour thumb) => new()
    {
        TrackFactory = r => new ColourGraphic(track, r.X, r.Y, r.W, r.H),
        Thumb = ButtonTheme.FromColour(thumb),
    };

    /// <summary>Builds a theme with a textured track and a textured thumb (with state colours applied as tints).</summary>
    public static ScrollbarTheme FromTextures(ITexture track, ITexture thumb) => new()
    {
        TrackFactory = r => new TextureGraphic(track, r.X, r.Y, r.W, r.H),
        Thumb = ButtonTheme.FromTexture(thumb),
    };
}
