using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
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
    private readonly bool _scrollOnTrackClick = false;
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
    /// <param name="scrollOnMouseWheel">When true, scroll wheel movement over this scrollbar adjusts <see cref="AmountFilled"/> by <see cref="WheelScrollStep"/> per notch and consumes the wheel event. Defaults to true.</param>
    /// <param name="scrollOnTrackClick">When true, left-clicking the track outside the thumb adjusts <see cref="AmountFilled"/> by <see cref="TrackClickStep"/> in the click's direction. Defaults to true.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Scrollbar(
        float x,
        float y,
        float width,
        float height,
        ScrollbarDirection direction,
        ScrollbarTheme theme,
        float thumbProportion = 0.2f,
        float amountFilled = 0f,
        bool scrollOnMouseWheel = true,
        bool scrollOnTrackClick = true,
        int layer = 0)
        : base(x, y, width, height, clickable: scrollOnMouseWheel || scrollOnTrackClick, layer: layer)
    {
        _direction = direction;
        _scrollOnTrackClick = scrollOnTrackClick;
        _thumbProportion = Math.Clamp(thumbProportion, MinThumbProportion, 1f);
        _amountFilled = Math.Clamp(amountFilled, 0f, 1f);

        if (scrollOnMouseWheel)
        {
            InterceptsMouseScroll = true;
        }

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

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="direction">Whether the thumb travels horizontally or vertically.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="thumbProportion">Thumb size as a fraction of the track length (along the scroll axis), in [0.05, 1]. Defaults to 0.2 (20%).</param>
    /// <param name="amountFilled">Initial scroll position in [0, 1]. Defaults to 0.</param>
    /// <param name="scrollOnMouseWheel">When true, scroll wheel movement over this scrollbar adjusts <see cref="AmountFilled"/> by <see cref="WheelScrollStep"/> per notch and consumes the wheel event. Defaults to true.</param>
    /// <param name="scrollOnTrackClick">When true, left-clicking the track outside the thumb adjusts <see cref="AmountFilled"/> by <see cref="TrackClickStep"/> in the click's direction. Defaults to true.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Scrollbar(
        Rect rect,
        ScrollbarDirection direction,
        ScrollbarTheme theme,
        float thumbProportion = 0.2f,
        float amountFilled = 0f,
        bool scrollOnMouseWheel = true,
        bool scrollOnTrackClick = true,
        int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, direction, theme, thumbProportion, amountFilled, scrollOnMouseWheel, scrollOnTrackClick, layer)
    {
    }

    internal Scrollbar(
        float width,
        float height,
        ScrollbarDirection direction,
        float thumbProportion = 0.2f,
        float amountFilled = 0f,
        bool scrollOnMouseWheel = false,
        bool scrollOnTrackClick = false)
        : base(0, 0, width, height, clickable: scrollOnMouseWheel || scrollOnTrackClick)
    {
        _direction = direction;
        _scrollOnTrackClick = scrollOnTrackClick;
        _thumbProportion = Math.Clamp(thumbProportion, MinThumbProportion, 1f);
        _amountFilled = Math.Clamp(amountFilled, 0f, 1f);

        if (scrollOnMouseWheel)
        {
            InterceptsMouseScroll = true;
        }

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

    /// <summary>
    /// How much <see cref="AmountFilled"/> changes per scroll wheel notch when
    /// <see cref="Entity.InterceptsMouseScroll"/> is true. Defaults to 0.1 (10% of the track).
    /// Scrolling up decreases <see cref="AmountFilled"/>; scrolling down increases it.
    /// </summary>
    /// <remarks>
    /// Wheel delta is mapped via <see cref="Math.Sign(float)"/>, so both horizontal and vertical
    /// scrollbars respond to vertical wheel input — this is by design, since most mice have only
    /// a vertical wheel and users expect either scrollbar under the cursor to react. There is no
    /// separate horizontal-wheel binding; a vertical wheel tick adjusts whichever scrollbar
    /// claims the scroll event via <see cref="Entity.InterceptsMouseScroll"/>.
    /// </remarks>
    public float WheelScrollStep { get; set; } = 0.1f;

    /// <summary>
    /// How much <see cref="AmountFilled"/> changes per left-click on the track outside the
    /// thumb when <c>scrollOnTrackClick</c> was enabled at construction. Defaults to 0.2 (20%
    /// of the track). Clicking before the thumb decreases <see cref="AmountFilled"/>; clicking
    /// after the thumb increases it.
    /// </summary>
    public float TrackClickStep { get; set; } = 0.2f;

    /// <summary>Raised after <see cref="AmountFilled"/> changes — by user drag or direct assignment.</summary>
    public event EventHandler<ScrollChangedEventArgs>? ScrollChanged;

    /// <inheritdoc/>
    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();

        if (!_scrollOnTrackClick)
        {
            return;
        }

        var (thumbWindowX, thumbWindowY) = GetWindowCoordinates(_thumb.X, _thumb.Y);

        if (_direction == ScrollbarDirection.Horizontal)
        {
            if (Mouse.ClientX < thumbWindowX)
            {
                AmountFilled -= TrackClickStep;
            }
            else if (Mouse.ClientX >= thumbWindowX + _thumb.Width)
            {
                AmountFilled += TrackClickStep;
            }
        }
        else
        {
            if (Mouse.ClientY < thumbWindowY)
            {
                AmountFilled -= TrackClickStep;
            }
            else if (Mouse.ClientY >= thumbWindowY + _thumb.Height)
            {
                AmountFilled += TrackClickStep;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseScrolled(float delta)
    {
        base.OnMouseScrolled(delta);
        AmountFilled -= Math.Sign(delta) * WheelScrollStep;
    }

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
