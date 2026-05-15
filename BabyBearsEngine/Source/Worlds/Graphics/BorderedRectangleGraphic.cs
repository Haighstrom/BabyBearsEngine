namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A solid rectangle with a coloured border drawn around its perimeter. Composes two
/// <see cref="ColourGraphic"/> children: a full-size rect for the border, and a smaller
/// inset rect for the fill.
/// </summary>
public class BorderedRectangleGraphic : Entity
{
    private readonly ColourGraphic _border;
    private readonly ColourGraphic _fill;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="borderThickness">Border width in pixels on each side.</param>
    /// <param name="fillColour">Colour of the interior.</param>
    /// <param name="borderColour">Colour of the border.</param>
    public BorderedRectangleGraphic(float x, float y, float width, float height, float borderThickness, Colour fillColour, Colour borderColour)
        : base(x, y, width, height)
    {
        _border = new ColourGraphic(borderColour, 0, 0, width, height);
        _fill = new ColourGraphic(fillColour, borderThickness, borderThickness, width - 2 * borderThickness, height - 2 * borderThickness);
        Add(_border);
        Add(_fill);
    }

    /// <summary>Colour of the border.</summary>
    public Colour BorderColour
    {
        get => _border.Colour;
        set => _border.Colour = value;
    }

    /// <summary>Colour of the interior fill.</summary>
    public Colour FillColour
    {
        get => _fill.Colour;
        set => _fill.Colour = value;
    }
}
