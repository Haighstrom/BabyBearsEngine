using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A drag-thumb scrollbar. The thumb's position along the track maps to
/// <see cref="AmountFilled"/> in [0, 1] — 0 places the thumb at the start of the track
/// (left for horizontal, top for vertical), 1 places it at the end. The thumb's size is a
/// fixed fraction of the track length, set at construction.
/// </summary>
/// <remarks>
/// Internally the thumb is a <see cref="Button"/>, so it gets idle / hover / pressed
/// tinting for free from its <see cref="ButtonTheme"/>. A <see cref="DragController"/>
/// watches the thumb's window-space rectangle and maps drag positions back to
/// <see cref="AmountFilled"/>.
/// </remarks>
public class Scrollbar : Entity
{
    private const float MinThumbProportion = 0.05f;

    private readonly ScrollbarDirection _direction;
    private readonly Button _thumb;
    private float _thumbProportion;
    private float _amountFilled = 0f;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="direction">Whether the thumb travels horizontally or vertically.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="thumbProportion">Thumb size as a fraction of the track length (along the scroll axis), in [0.05, 1]. Defaults to 0.2 (20%).</param>
    /// <param name="amountFilled">Initial scroll position in [0, 1]. Defaults to 0.</param>
    public Scrollbar(
        float x,
        float y,
        float width,
        float height,
        ScrollbarDirection direction,
        ScrollbarTheme theme,
        float thumbProportion = 0.2f,
        float amountFilled = 0f)
        : base(x, y, width, height)
    {
        _direction = direction;
        _thumbProportion = Math.Clamp(thumbProportion, MinThumbProportion, 1f);
        _amountFilled = Math.Clamp(amountFilled, 0f, 1f);

        Add(theme.TrackFactory(new Rect(0, 0, width, height)));

        (float tx, float ty, float tw, float th) = ComputeThumbRect(_amountFilled);
        _thumb = new Button(tx, ty, tw, th, theme.Thumb);
        Add(_thumb);

        DragController controller = new(
            grabArea: () =>
            {
                var (wx, wy) = GetWindowCoordinates(_thumb.X, _thumb.Y);
                return new Rect(wx, wy, _thumb.Width, _thumb.Height);
            },
            getPosition: () =>
            {
                var (wx, wy) = GetWindowCoordinates(_thumb.X, _thumb.Y);
                return ((int)wx, (int)wy);
            });
        controller.PositionChanged += OnDragPositionChanged;
        Add(controller);
    }

    internal Scrollbar(float width, float height, ScrollbarDirection direction, float thumbProportion = 0.2f, float amountFilled = 0f)
        : base(0, 0, width, height)
    {
        _direction = direction;
        _thumbProportion = Math.Clamp(thumbProportion, MinThumbProportion, 1f);
        _amountFilled = Math.Clamp(amountFilled, 0f, 1f);

        (float tx, float ty, float tw, float th) = ComputeThumbRect(_amountFilled);
        _thumb = new Button(tx, ty, tw, th);
        Add(_thumb);
    }

    /// <summary>
    /// The thumb's position along the track, in [0, 1]. Values outside the range are clamped.
    /// Setting this raises <see cref="ScrollChanged"/> if the value changes.
    /// </summary>
    public float AmountFilled
    {
        get => _amountFilled;
        set
        {
            float clamped = Math.Clamp(value, 0f, 1f);

            if (_amountFilled == clamped)
            {
                return;
            }

            float old = _amountFilled;
            _amountFilled = clamped;
            UpdateThumbPosition();
            ScrollChanged?.Invoke(this, new ScrollChangedEventArgs(old, clamped));
        }
    }

    /// <summary>
    /// The thumb's size as a fraction of the track length, in [<see cref="MinThumbProportion"/>, 1].
    /// Setting this resizes and repositions the thumb.
    /// </summary>
    public float ThumbProportion
    {
        get => _thumbProportion;
        set
        {
            _thumbProportion = Math.Clamp(value, MinThumbProportion, 1f);
            UpdateThumb();
        }
    }

    /// <summary>Raised after <see cref="AmountFilled"/> changes — by user drag or direct assignment.</summary>
    public event EventHandler<ScrollChangedEventArgs>? ScrollChanged;

    private (float X, float Y, float W, float H) ComputeThumbRect(float amountFilled)
    {
        if (_direction == ScrollbarDirection.Horizontal)
        {
            float thumbW = Width * _thumbProportion;
            float thumbX = (Width - thumbW) * amountFilled;
            return (thumbX, 0f, thumbW, Height);
        }

        float thumbH = Height * _thumbProportion;
        float thumbY = (Height - thumbH) * amountFilled;
        return (0f, thumbY, Width, thumbH);
    }

    private void OnDragPositionChanged(int newWindowX, int newWindowY)
    {
        var (scrollbarWX, scrollbarWY) = GetWindowCoordinates(0, 0);

        if (_direction == ScrollbarDirection.Horizontal)
        {
            float thumbW = Width * _thumbProportion;
            float maxThumbX = Width - thumbW;
            if (maxThumbX <= 0)
            {
                return;
            }

            float newLocalX = newWindowX - scrollbarWX;
            float clamped = Math.Clamp(newLocalX, 0, maxThumbX);
            AmountFilled = clamped / maxThumbX;
        }
        else
        {
            float thumbH = Height * _thumbProportion;
            float maxThumbY = Height - thumbH;
            if (maxThumbY <= 0)
            {
                return;
            }

            float newLocalY = newWindowY - scrollbarWY;
            float clamped = Math.Clamp(newLocalY, 0, maxThumbY);
            AmountFilled = clamped / maxThumbY;
        }
    }

    private void UpdateThumb()
    {
        if (_direction == ScrollbarDirection.Horizontal)
        {
            float thumbW = Width * _thumbProportion;
            _thumb.Width = thumbW;
            _thumb.X = (Width - thumbW) * _amountFilled;
        }
        else
        {
            float thumbH = Height * _thumbProportion;
            _thumb.Height = thumbH;
            _thumb.Y = (Height - thumbH) * _amountFilled;
        }
    }

    private void UpdateThumbPosition()
    {
        if (_direction == ScrollbarDirection.Horizontal)
        {
            float thumbW = Width * _thumbProportion;
            _thumb.X = (Width - thumbW) * _amountFilled;
        }
        else
        {
            float thumbH = Height * _thumbProportion;
            _thumb.Y = (Height - thumbH) * _amountFilled;
        }
    }
}
