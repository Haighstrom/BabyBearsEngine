using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>A container entity with a solid coloured background.</summary>
public class Panel : Entity
{
    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="colour">Background fill colour.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Panel(float x, float y, float width, float height, Colour colour, int layer = 0)
        : base(x, y, width, height, layer: layer)
    {
        Add(new ColourGraphic(colour, 0, 0, width, height));
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="colour">Background fill colour.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Panel(Rect rect, Colour colour, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, colour, layer)
    {
    }
}
