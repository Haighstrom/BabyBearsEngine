namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A rectangular border frame drawn as four solid strips — top, bottom, left, and right.
/// The interior is empty; place a <see cref="ColourGraphic"/> or other graphic behind this
/// entity if a fill is needed. The <see cref="BorderPosition"/> controls whether the strips
/// draw inside, outside, or centred on the graphic's stated rectangle.
/// </summary>
public class BorderedRectangleGraphic : Entity, IBorderGraphic
{
    private readonly ColourGraphic _bottom;
    private Colour _borderColour;
    private BorderPosition _borderPosition;
    private float _borderThickness;
    private readonly ColourGraphic _left;
    private readonly ColourGraphic _right;
    private readonly ColourGraphic _top;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="borderThickness">Width of each border strip in pixels.</param>
    /// <param name="borderColour">Colour of the border.</param>
    /// <param name="borderPosition">Controls whether the border draws inside, outside, or centred on the stated bounds. Defaults to <see cref="BorderPosition.Inside"/>.</param>
    public BorderedRectangleGraphic(float x, float y, float width, float height, float borderThickness,
        Colour borderColour, BorderPosition borderPosition = BorderPosition.Inside)
        : base(x, y, width, height)
    {
        _borderThickness = borderThickness;
        _borderColour = borderColour;
        _borderPosition = borderPosition;
        _top = new ColourGraphic(borderColour, 0f, 0f, 0f, 0f);
        _bottom = new ColourGraphic(borderColour, 0f, 0f, 0f, 0f);
        _left = new ColourGraphic(borderColour, 0f, 0f, 0f, 0f);
        _right = new ColourGraphic(borderColour, 0f, 0f, 0f, 0f);
        Add(_top);
        Add(_bottom);
        Add(_left);
        Add(_right);
        UpdateLayout();
    }

    /// <summary>Colour of the border strips.</summary>
    public Colour BorderColour
    {
        get => _borderColour;
        set
        {
            _borderColour = value;
            _top.Colour = value;
            _bottom.Colour = value;
            _left.Colour = value;
            _right.Colour = value;
        }
    }

    /// <summary>
    /// Controls whether the border draws inside, outside, or centred on the stated bounds.
    /// Changing this recomputes the strip layout.
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

    /// <summary>Width of each border strip in pixels. Changing this recomputes the strip layout.</summary>
    public float BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = value;
            UpdateLayout();
        }
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (_top is not null)
        {
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        float t = _borderThickness;
        float w = Width;
        float h = Height;

        switch (_borderPosition)
        {
            case BorderPosition.Inside:
                _top.X = 0f;     _top.Y = 0f;     _top.Width = w;     _top.Height = t;
                _bottom.X = 0f;  _bottom.Y = h - t; _bottom.Width = w; _bottom.Height = t;
                _left.X = 0f;    _left.Y = t;       _left.Width = t;   _left.Height = h - 2f * t;
                _right.X = w - t; _right.Y = t;     _right.Width = t;  _right.Height = h - 2f * t;
                break;

            case BorderPosition.Outside:
                _top.X = -t;    _top.Y = -t;    _top.Width = w + 2f * t;    _top.Height = t;
                _bottom.X = -t; _bottom.Y = h;  _bottom.Width = w + 2f * t; _bottom.Height = t;
                _left.X = -t;   _left.Y = 0f;   _left.Width = t;            _left.Height = h;
                _right.X = w;   _right.Y = 0f;  _right.Width = t;           _right.Height = h;
                break;

            case BorderPosition.Centred:
                float half = t / 2f;
                _top.X = -half;    _top.Y = -half;    _top.Width = w + t;    _top.Height = t;
                _bottom.X = -half; _bottom.Y = h - half; _bottom.Width = w + t; _bottom.Height = t;
                _left.X = -half;   _left.Y = half;    _left.Width = t;       _left.Height = h - t;
                _right.X = w - half; _right.Y = half; _right.Width = t;      _right.Height = h - t;
                break;
        }
    }
}
