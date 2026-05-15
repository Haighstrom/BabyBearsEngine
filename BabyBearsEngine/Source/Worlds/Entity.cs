using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Standard game entity. Has a position and size inherited from <see cref="AddableRectBase"/>,
/// can hold child entities/graphics (inherited from <see cref="ContainerEntity"/>), and
/// optionally exposes mouse interaction events when constructed with <c>clickable: true</c>.
/// </summary>
public class Entity : ContainerEntity, IMouseInteractable
{
    private const double HoverDelaySeconds = 0.5;

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
            _clickController = new(this, HoverDelaySeconds);
            _clickController.HoverCancelled += OnStopMouseHovered;
            _clickController.Hovered += OnMouseHovered;
            _clickController.LeftPressed += OnLeftPressed;
            _clickController.LeftReleased += OnLeftReleased;
            _clickController.MouseEntered += OnMouseEntered;
            _clickController.MouseExited += OnMouseExited;
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

    /// <inheritdoc/>
    public Rect PositionOnScreen
    {
        get
        {
            var (wx, wy) = Parent?.GetWindowCoordinates(X, Y) ?? (X, Y);
            return new(wx, wy, Width, Height);
        }
    }

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

    /// <summary>Raised when the left mouse button is pressed while the cursor is over this entity. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? LeftPressed;

    /// <summary>Raised when the left mouse button is released while the cursor is over this entity. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? LeftReleased;

    /// <summary>Raised when the cursor enters this entity's bounds. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseEntered;

    /// <summary>Raised when the cursor leaves this entity's bounds. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseExited;

    /// <summary>Raised when the cursor has rested over this entity for the hover delay (~0.5 s). Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseHovered;

    /// <summary>Raised when the cursor moves off this entity after a hover. Requires <c>clickable: true</c>.</summary>
    public event EventHandler? MouseHoverStopped;

    /// <summary>Raises <see cref="LeftPressed"/>. Override to customise without subscribing.</summary>
    protected virtual void OnLeftPressed()
    {
        LeftPressed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="LeftReleased"/>. Override to customise without subscribing.</summary>
    protected virtual void OnLeftReleased()
    {
        LeftReleased?.Invoke(this, EventArgs.Empty);
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

    /// <summary>Raises <see cref="MouseHoverStopped"/>. Override to customise without subscribing.</summary>
    protected virtual void OnStopMouseHovered()
    {
        MouseHoverStopped?.Invoke(this, EventArgs.Empty);
    }
}
