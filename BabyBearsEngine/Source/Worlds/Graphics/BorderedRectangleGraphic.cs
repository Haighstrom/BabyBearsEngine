namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A solid rectangle with a coloured border drawn around its perimeter. Composes two
/// <see cref="ColourGraphic"/> children: one for the border and one for the fill.
/// The <see cref="BorderPosition"/> controls whether the border draws inside, outside,
/// or centred on the graphic's stated rectangle.
/// </summary>
public class BorderedRectangleGraphic : Entity
{
    private readonly ColourGraphic _border;
    private BorderPosition _borderPosition;
    private float _borderThickness;
    private readonly ColourGraphic _fill;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="borderThickness">Border width in pixels on each side.</param>
    /// <param name="fillColour">Colour of the interior.</param>
    /// <param name="borderColour">Colour of the border.</param>
    /// <param name="borderPosition">Controls whether the border draws inside, outside, or centred on the stated bounds. Defaults to <see cref="BorderPosition.Inside"/>.</param>
    public BorderedRectangleGraphic(float x, float y, float width, float height, float borderThickness, Colour fillColour, Colour borderColour, BorderPosition borderPosition = BorderPosition.Inside)
        : base(x, y, width, height)
    {
        _borderThickness = borderThickness;
        _borderPosition = borderPosition;
        _border = new ColourGraphic(borderColour, 0f, 0f, 0f, 0f);
        _fill = new ColourGraphic(fillColour, 0f, 0f, 0f, 0f);
        Add(_border);
        Add(_fill);
        UpdateLayout();
    }

    /// <summary>Colour of the border.</summary>
    public Colour BorderColour
    {
        get => _border.Colour;
        set => _border.Colour = value;
    }

    /// <summary>
    /// Controls whether the border draws inside, outside, or centred on the stated bounds.
    /// Changing this recomputes the child layout.
    /// </summary>
    public BorderPosition BorderPosition
    {
        get => _borderPosition;
        set
        {
            _borderPosition = value;
            UpdateLayout();
        }
    }

    /// <summary>
    /// Border width in pixels on each side. Changing this recomputes the child layout.
    /// </summary>
    public float BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = value;
            UpdateLayout();
        }
    }

    /// <summary>
    /// Sets the colour of both the border and the fill simultaneously. Getting returns the border colour.
    /// Useful for tinting the whole graphic; if the fill is transparent the setter has no visible effect on it.
    /// </summary>
    public Colour Colour
    {
        get => _border.Colour;
        set
        {
            _border.Colour = value;
            _fill.Colour = value;
        }
    }

    /// <summary>Colour of the interior fill.</summary>
    public Colour FillColour
    {
        get => _fill.Colour;
        set => _fill.Colour = value;
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (_border is not null)
            UpdateLayout();
    }

    private void UpdateLayout()
    {
        float t = _borderThickness;
        float w = Width;
        float h = Height;

        switch (_borderPosition)
        {
            case BorderPosition.Inside:
                _border.X = 0f;
                _border.Y = 0f;
                _border.Width = w;
                _border.Height = h;
                _fill.X = t;
                _fill.Y = t;
                _fill.Width = w - 2f * t;
                _fill.Height = h - 2f * t;
                break;

            case BorderPosition.Outside:
                _border.X = -t;
                _border.Y = -t;
                _border.Width = w + 2f * t;
                _border.Height = h + 2f * t;
                _fill.X = 0f;
                _fill.Y = 0f;
                _fill.Width = w;
                _fill.Height = h;
                break;

            case BorderPosition.Centred:
                float half = t / 2f;
                _border.X = -half;
                _border.Y = -half;
                _border.Width = w + t;
                _border.Height = h + t;
                _fill.X = half;
                _fill.Y = half;
                _fill.Width = w - t;
                _fill.Height = h - t;
                break;
        }
    }
}
