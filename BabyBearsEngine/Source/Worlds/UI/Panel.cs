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
    public Panel(float x, float y, float width, float height, Colour colour)
        : base(x, y, width, height)
    {
        Add(new ColourGraphic(colour, 0, 0, width, height));
    }
}
