using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A solid-colour filled rectangle with a border drawn on top — a <see cref="ColourGraphic"/>
/// fill behind a <see cref="BorderedRectangleGraphic"/> frame. Resizing the graphic resizes
/// both. It is a composite entity but still satisfies <see cref="IGraphic"/>, so it can be
/// used anywhere a single graphic is expected (e.g. a <see cref="UI.ProgressBar"/> fill).
/// </summary>
public class BorderedColourGraphic : Entity, IGraphic
{
    private readonly BorderedRectangleGraphic _border;
    private readonly ColourGraphic _fill;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="fillColour">Colour of the filled interior.</param>
    /// <param name="borderColour">Colour of the border frame.</param>
    /// <param name="borderThickness">Width of each border strip in pixels.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public BorderedColourGraphic(float x, float y, float width, float height, Colour fillColour,
        Colour borderColour, float borderThickness, int layer = 0)
        : base(x, y, width, height, layer: layer)
    {
        _fill = new ColourGraphic(fillColour, 0f, 0f, width, height);
        _border = new BorderedRectangleGraphic(0f, 0f, width, height, borderThickness, borderColour);
        Add(_fill);
        Add(_border);
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="fillColour">Colour of the filled interior.</param>
    /// <param name="borderColour">Colour of the border frame.</param>
    /// <param name="borderThickness">Width of each border strip in pixels.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public BorderedColourGraphic(Rect rect, Colour fillColour, Colour borderColour, float borderThickness, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, fillColour, borderColour, borderThickness, layer)
    {
    }

    /// <summary>Colour of the filled interior. The border colour is fixed at construction.</summary>
    public Colour Colour
    {
        get => _fill.Colour;
        set => _fill.Colour = value;
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (_fill is not null)
        {
            _fill.Width = Width;
            _fill.Height = Height;
            _border.Width = Width;
            _border.Height = Height;
        }
    }
}
