using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Standard game entity. Has a position and size inherited from <see cref="AddableRectBase"/>,
/// can hold child entities/graphics (inherited from <see cref="ContainerEntity"/>), and
/// optionally exposes mouse interaction events when constructed with <c>clickable: true</c>.
/// </summary>
/// <remarks>
/// When <c>clickable: true</c> the entity owns an internal <c>ClickController</c> child and wires
/// nine event subscriptions from controller-event → entity-method. These subscriptions are
/// installed once at construction and are never unwired — the controller is private and
/// inseparable from this entity for its whole lifetime: the two are constructed together, share
/// a parent-child link, and are released to GC together. Don't reparent the controller, and
/// don't read or store it externally — there's no supported path that would survive the entity.
/// Removing the entity from its tree leaves the controller as a child of the (now-detached)
/// entity, so re-adding the entity re-introduces the controller and the subscriptions still
/// point at the same entity methods.
/// </remarks>
public class Entity : ContainerEntity, IMouseInteractable
{
    private readonly ClickController? _clickController = null;

    /// <param name="x">X position relative to parent.</param>
    /// <param name="y">Y position relative to parent.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="clickable">When true, adds an internal click controller that raises the mouse interaction events (<see cref="LeftPressed"/>, <see cref="MouseEntered"/>, <see cref="MouseHovered"/>, etc.).</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Entity(float x, float y, float width, float height, bool clickable = false, int layer = 0)
        : base(x, y, width, height, layer)
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
            _clickController.ScrollWheelMoved += OnMouseScrolled;
            Add(_clickController);
        }
    }

    /// <param name="rect">Position and size relative to parent.</param>
    /// <param name="clickable">When true, adds an internal click controller that raises the mouse interaction events (<see cref="LeftPressed"/>, <see cref="MouseEntered"/>, <see cref="MouseHovered"/>, etc.).</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public Entity(Rect rect, bool clickable = false, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, clickable, layer)
    {
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
    /// When true and the mouse is over this entity, scroll wheel movement fires
    /// <see cref="MouseScrolled"/> and sets <see cref="MouseSolver.WheelScrollConsumed"/>,
    /// preventing world-level scroll handlers from also reacting to the wheel that frame.
    /// Requires <c>clickable: true</c>.
    /// </summary>
    public bool InterceptsMouseScroll
    {
        get => _clickController?.InterceptsMouseScroll ?? false;
        set
        {
            if (_clickController is not null)
            {
                _clickController.InterceptsMouseScroll = value;
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
    /// <exception cref="InvalidOperationException">Thrown when <see cref="AddableBase.Parent"/> is <c>null</c> — an entity outside the entity tree has no screen position.</exception>
    public Rect PositionOnScreen
    {
        get
        {
            var (wx, wy) = Parent?.GetWindowCoordinates(X, Y) ?? throw new InvalidOperationException("PositionOnScreen requires Parent — entity is not in an entity tree (never added, or removed).");
            return new(wx, wy, Width, Height);
        }
    }

    /// <inheritdoc cref="IMouseInteractable.HitRect"/>
    public virtual Rect HitRect => PositionOnScreen;

    /// <inheritdoc cref="IMouseInteractable.Disabled"/>
    /// <remarks>
    /// Distinct from <see cref="IUpdateable.Active"/> — Active = false halts per-frame updates,
    /// while Disabled = true halts mouse interaction. A common pattern is to set both on a
    /// "frozen" entity, but either flag can be flipped independently.
    /// </remarks>
    public virtual bool Disabled { get; set; } = false;

    /// <summary>
    /// Translates a point from this entity's local space to window space by adding this entity's
    /// position and walking up the parent chain.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="AddableBase.Parent"/> is <c>null</c> — an entity outside the entity tree has no window-space position to translate into.</exception>
    public override (float x, float y) GetWindowCoordinates(float x, float y) =>
        Parent?.GetWindowCoordinates(x + X, y + Y) ?? throw new InvalidOperationException("GetWindowCoordinates requires Parent — entity is not in an entity tree (never added, or removed).");

    /// <summary>
    /// Renders all child renderables, applying this entity's (X, Y) translation to the model-view matrix first.
    /// Entity subclasses should normally add their own rendering by adding child <see cref="IRenderable"/>s
    /// rather than overriding here. The override hook exists so composite <see cref="Graphics.IGraphic"/>
    /// implementers (e.g. <see cref="Graphics.BorderedColourGraphic"/>) can apply transforms such as
    /// rotation to the model-view matrix before children render.
    /// </summary>
    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
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

    /// <summary>Raised when the scroll wheel moves while the cursor is over this entity and <see cref="InterceptsMouseScroll"/> is true. Requires <c>clickable: true</c>.</summary>
    public event EventHandler<MouseScrolledEventArgs>? MouseScrolled;

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

    /// <summary>Raises <see cref="MouseScrolled"/>. Override to customise without subscribing.</summary>
    protected virtual void OnMouseScrolled(float delta)
    {
        MouseScrolled?.Invoke(this, new MouseScrolledEventArgs(delta));
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
