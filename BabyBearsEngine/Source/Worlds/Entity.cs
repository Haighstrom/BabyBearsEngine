using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Standard game entity. Has a position and size inherited from <see cref="AddableRectBase"/>,
/// can hold child entities/graphics (inherited from <see cref="ContainerEntity"/>), and
/// optionally exposes mouse interaction events when constructed with <c>clickable: true</c>.
/// </summary>
public class Entity : ContainerEntity, IMouseInteractable
{
    private readonly ClickController? _clickController = null;

    /// <param name="x">X position relative to parent.</param>
    /// <param name="y">Y position relative to parent.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="clickable">When true, adds an internal click controller that raises the mouse interaction events (<see cref="LeftPressed"/>, <see cref="MouseEntered"/>, <see cref="MouseHovered"/>, etc.).</param>
    public Entity(float x, float y, float width, float height, bool clickable = false)
        : base(x, y, width, height)
    {
        if (clickable)
        {
            _clickController = new(this);
            _clickController.HoverCancelled += OnStopMouseHovered;
            _clickController.Hovered += OnMouseHovered;
            _clickController.LeftClicked += OnLeftClicked;
            _clickController.LeftDoubleClicked += OnLeftDoubleClicked;
            _clickController.LeftPressed += OnLeftPressed;
            _clickController.MouseEntered += OnMouseEntered;
            _clickController.MouseExited += OnMouseExited;
            _clickController.RightClicked += OnRightClicked;
            _clickController.RightPressed += OnRightPressed;
            Add(_clickController);
        }
    }

    /// <summary>
    /// When true, mouse-over state propagates through this entity to overlapping clickable
    /// entities beneath it rather than stopping here. Only meaningful when clickable.
    /// </summary>
    public bool ClickThrough
    {
        get => _clickController?.ClickThrough ?? false;
        set
        {
            if (_clickController is not null)
            {
                _clickController.ClickThrough = value;
            }
        }
    }

    /// <summary>
    /// When true, a double-click also fires <see cref="LeftClicked"/> for the second click
    /// in addition to <see cref="LeftDoubleClicked"/>. Requires <c>clickable: true</c>.
    /// </summary>
    public bool DoubleClickTriggersSingleClick
    {
        get => _clickController?.DoubleClickTriggersSingleClick ?? true;
        set
        {
            if (_clickController is not null)
            {
                _clickController.DoubleClickTriggersSingleClick = value;
            }
        }
    }

    /// <summary>
    /// Maximum time in seconds between two left-clicks for them to count as a double-click.
    /// Default is 0.5 seconds. Requires <c>clickable: true</c>.
    /// </summary>
    public double DoubleClickWindow
    {
        get => _clickController?.DoubleClickWindow ?? 0.5;
        set
        {
            if (_clickController is not null)
            {
                _clickController.DoubleClickWindow = value;
            }
        }
    }

    /// <summary>
    /// Seconds the cursor must rest over this entity before <see cref="MouseHovered"/> fires.
    /// Default is 0.5. Set to 0 to fire immediately on mouse enter. Requires <c>clickable: true</c>.
    /// </summary>
    public double HoverDelay
    {
        get => _clickController?.HoverDelay ?? throw new InvalidOperationException("HoverDelay requires clickable: true.");
        set
        {
            if (_clickController is not null)
            {
                _clickController.HoverDelay = value;
            }
        }
    }

    /// <inheritdoc/>
    public Rect PositionOnScreen
    {
        get
        {
            var (wx, wy) = Parent?.GetWindowCoordinates(X, Y) ?? (X, Y);
            return new(wx, wy, Width, Height);
        }
    }

    /// <inheritdoc cref="IMouseInteractable.HitRect"/>
    public virtual Rect HitRect => PositionOnScreen;

    /// <summary>
    /// Translates a point from this entity's local space to window space by adding this entity's
    /// position and walking up the parent chain.
    /// </summary>
    public override (float x, float y) GetWindowCoordinates(float x, float y) =>
        Parent?.GetWindowCoordinates(x + X, y + Y) ?? (x + X, y + Y);

    /// <summary>
    /// Renders all child renderables, applying this entity's (X, Y) translation to the model-view matrix first.
    /// Sealed — entity subclasses should add their own rendering by adding child <see cref="IRenderable"/>s rather than overriding here.
    /// </summary>
    public sealed override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        var mv = Matrix3.Translate(ref modelView, X, Y);
        base.Render(ref projection, ref mv);
    }

    /// <summary>Raised when a left click completes on this entity (pressed and released while over it). Requires <c>clickable: true</c>.</summary>
    public event EventHandler? LeftClicked;

    /// <summary>Raised when two left-clicks occur within <see cref="DoubleClickWindow"/> seconds of each other. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? LeftDoubleClicked;

    /// <summary>Raised when the left mouse button is pressed while the cursor is over this entity. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? LeftPressed;

    /// <summary>Raised when the cursor enters this entity's bounds. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseEntered;

    /// <summary>Raised when the cursor leaves this entity's bounds. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseExited;

    /// <summary>Raised when the cursor has rested over this entity for <see cref="HoverDelay"/> seconds. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseHovered;

    /// <summary>Raised when the cursor moves off this entity after a hover. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseHoverStopped;

    /// <summary>Raised when a right click completes on this entity (pressed and released while over it). Requires <c>clickable: true</c>.</summary>
    public event EventHandler? RightClicked;

    /// <summary>Raised when the right mouse button is pressed while the cursor is over this entity. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? RightPressed;

    /// <summary>Raises <see cref="LeftClicked"/>. Override to customise without subscribing.</summary>
    protected virtual void OnLeftClicked()
    {
        LeftClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="LeftDoubleClicked"/>. Override to customise without subscribing.</summary>
    protected virtual void OnLeftDoubleClicked()
    {
        LeftDoubleClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="LeftPressed"/>. Override to customise without subscribing.</summary>
    protected virtual void OnLeftPressed()
    {
        LeftPressed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="MouseEntered"/>. Override to customise without subscribing.</summary>
    protected virtual void OnMouseEntered()
    {
        MouseEntered?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="MouseExited"/>. Override to customise without subscribing.</summary>
    protected virtual void OnMouseExited()
    {
        MouseExited?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="MouseHovered"/>. Override to customise without subscribing.</summary>
    protected virtual void OnMouseHovered()
    {
        MouseHovered?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="RightClicked"/>. Override to customise without subscribing.</summary>
    protected virtual void OnRightClicked()
    {
        RightClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="RightPressed"/>. Override to customise without subscribing.</summary>
    protected virtual void OnRightPressed()
    {
        RightPressed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="MouseHoverStopped"/>. Override to customise without subscribing.</summary>
    protected virtual void OnStopMouseHovered()
    {
        MouseHoverStopped?.Invoke(this, EventArgs.Empty);
    }
}
