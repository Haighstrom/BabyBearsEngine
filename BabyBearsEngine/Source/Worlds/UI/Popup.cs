using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="Panel"/> that implements <see cref="IMenu"/>: starts hidden and can be shown or
/// hidden via <see cref="Open"/> / <see cref="Close"/>. Typically added to
/// <see cref="IWorld.Overlay"/> so it renders above all other widgets, then registered with a
/// <see cref="IMenuGroup"/> to enforce mutual exclusivity.
/// </summary>
public class Popup : Panel, IMenu
{
    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="colour">Background fill colour.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Popup(float x, float y, float width, float height, Colour colour, int layer = 0)
        : base(x, y, width, height, colour, layer)
    {
        Visible = false;
        Active = false;
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="colour">Background fill colour.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Popup(Rect rect, Colour colour, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, colour, layer)
    {
    }

    // Skips ColourGraphic creation for unit tests that run without an OpenGL context.
    internal Popup(float x, float y, float width, float height)
        : base(x, y, width, height)
    {
        Visible = false;
        Active = false;
    }

    /// <inheritdoc/>
    public bool IsOpen { get; private set; } = false;

    /// <inheritdoc/>
    public void Open()
    {
        IsOpen = true;
        Visible = true;
        Active = true;
    }

    /// <inheritdoc/>
    public void Close()
    {
        IsOpen = false;
        Visible = false;
        Active = false;
    }
}
