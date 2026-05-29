using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Controls mouse interactions for a clickable region, raising events for each
/// interaction. Implements a state machine to handle enter/exit, hover, press/release,
/// and the case where the user presses inside then drags outside while holding the button.
/// <para>
/// Call <see cref="Update(double)"/> each frame. The controller calls <see cref="IMouseInteractable.PositionOnScreen"/>
/// on the supplied target to determine whether the mouse is over the region, and registers
/// with <see cref="MouseSolver"/> so that only the top-most overlapping controller receives input.
/// </para>
/// <para>
/// <see cref="LeftClicked"/> is only raised when the mouse was pressed while over the
/// region and released while still over it (a successful click). Pressing inside, dragging
/// outside, and releasing raises <see cref="MouseExited"/> instead to signal cancellation.
/// </para>
/// </summary>
/// <param name="target">The entity to hit-test each frame.</param>
/// <param name="timeToTriggerHover">Seconds the mouse must remain over the region before <see cref="Hovered"/> is raised.</param>
internal sealed class ClickController(IMouseInteractable target, double timeToTriggerHover = 0.5) : AddableBase, IUpdateable, IClickController
{
    private enum ClickState
    {
        None,
        MouseOver,
        Hovering,
        MouseDownInside,
        MouseDownOutside,
    }

    private enum RightClickState
    {
        None,
        DownInside,
        DownOutside,
    }

    private ClickState _clickState = ClickState.None;
    private double _hoverTimeElapsed = 0;
    private bool _mouseIsOver = false;
    private RightClickState _rightClickState = RightClickState.None;
    private double _timeSinceLastClick = double.MaxValue;

    public bool Active { get; set; } = true;

    /// <summary>
    /// Seconds the cursor must remain over this region before <see cref="Hovered"/> fires.
    /// Defaults to 0.5. Set to 0 to fire immediately on mouse enter.
    /// </summary>
    public double HoverDelay { get; set; } = timeToTriggerHover;

    /// <summary>
    /// When true, <see cref="MouseSolver"/> continues propagating mouse-over state to
    /// overlapping controllers beneath this one instead of stopping here. Useful for
    /// transparent overlays that should not block clicks on entities below them.
    /// </summary>
    public bool ClickThrough { get; set; } = false;

    /// <summary>
    /// When true and the mouse is over this entity, scroll wheel movement fires
    /// <see cref="ScrollWheelMoved"/> and marks <see cref="MouseSolver.WheelScrollConsumed"/>,
    /// preventing world-level scroll handlers from also reacting to the wheel that frame.
    /// </summary>
    public bool InterceptsMouseScroll { get; set; } = false;

    /// <summary>
    /// When true, a double-click also fires <see cref="LeftClicked"/> for the second click
    /// in addition to <see cref="LeftDoubleClicked"/>. Default is true. Set to false only
    /// when a double-click should suppress the single-click (e.g. double-click opens a dialog,
    /// single click selects — where triggering both would be wrong).
    /// </summary>
    public bool DoubleClickTriggersSingleClick { get; set; } = true;

    /// <summary>
    /// Maximum time in seconds between two left-clicks for them to count as a double-click.
    /// Default is 0.5 seconds.
    /// </summary>
    public double DoubleClickWindow { get; set; } = 0.5;

    public event Action? HoverCancelled;
    public event Action? Hovered;
    internal event Action<float>? ScrollWheelMoved;
    public event Action? LeftClicked;
    public event Action? LeftDoubleClicked;
    public event Action? LeftPressed;
    public event Action? MouseEntered;
    public event Action? MouseExited;
    public event Action? RightClicked;
    public event Action? RightPressed;

    /// <summary>
    /// Called by <see cref="MouseSolver"/> to inform this controller whether it is
    /// the top-most entity under the mouse this frame.
    /// </summary>
    public void SetMouseOver(bool isMouseOver)
    {
        _mouseIsOver = isMouseOver;
    }

    /// <summary>Advance the controller state by <paramref name="elapsed"/> seconds.</summary>
    public void Update(double elapsed)
    {
        // A disabled target suppresses all mouse interaction — skip it entirely so neither
        // its events nor any subclass OnLeftClicked override fire. A target whose parent
        // chain doesn't reach a tree root (a non-IAddable IContainer like a World) is
        // detached, has no screen position to hit-test, and is treated the same as disabled.
        if (target.Disabled || IsDetached(target))
        {
            _clickState = ClickState.None;
            _rightClickState = RightClickState.None;
            return;
        }

        _timeSinceLastClick += elapsed;

        if (target.HitRect.Contains(Mouse.ClientX, Mouse.ClientY))
        {
            MouseSolver.RegisterMouseOver(this);
        }

        switch (_clickState)
        {
            case ClickState.None:
                if (_mouseIsOver)
                {
                    _clickState = ClickState.MouseOver;
                    _hoverTimeElapsed = 0;
                    MouseEntered?.Invoke();

                    if (Mouse.LeftPressed)
                    {
                        _clickState = ClickState.MouseDownInside;
                        LeftPressed?.Invoke();
                    }
                }
                break;

            case ClickState.MouseOver:
                if (!_mouseIsOver)
                {
                    _clickState = ClickState.None;
                    MouseExited?.Invoke();
                }
                else if (Mouse.LeftPressed)
                {
                    _clickState = ClickState.MouseDownInside;
                    LeftPressed?.Invoke();
                }
                else
                {
                    _hoverTimeElapsed += elapsed;

                    if (_hoverTimeElapsed >= HoverDelay)
                    {
                        _clickState = ClickState.Hovering;
                        Hovered?.Invoke();
                    }
                }
                break;

            case ClickState.Hovering:
                if (!_mouseIsOver)
                {
                    _clickState = ClickState.None;
                    HoverCancelled?.Invoke();
                    MouseExited?.Invoke();
                }
                else if (Mouse.LeftPressed)
                {
                    _clickState = ClickState.MouseDownInside;
                    HoverCancelled?.Invoke();
                    LeftPressed?.Invoke();
                }
                break;

            case ClickState.MouseDownInside:
                if (!_mouseIsOver)
                {
                    // Exit takes priority: cancel the interaction even if release also occurs this frame.
                    if (Mouse.LeftReleased)
                    {
                        _clickState = ClickState.None;
                        MouseExited?.Invoke();
                    }
                    else
                    {
                        _clickState = ClickState.MouseDownOutside;
                    }
                }
                else if (Mouse.LeftReleased)
                {
                    _clickState = ClickState.MouseOver;
                    _hoverTimeElapsed = 0;

                    if (_timeSinceLastClick <= DoubleClickWindow)
                    {
                        if (DoubleClickTriggersSingleClick)
                        {
                            LeftClicked?.Invoke();
                        }

                        LeftDoubleClicked?.Invoke();
                    }
                    else
                    {
                        LeftClicked?.Invoke();
                    }

                    _timeSinceLastClick = 0;
                }
                break;

            case ClickState.MouseDownOutside:
                if (Mouse.LeftReleased)
                {
                    _clickState = ClickState.None;
                    MouseExited?.Invoke();
                }
                else if (_mouseIsOver)
                {
                    _clickState = ClickState.MouseDownInside;
                }
                break;

            default:
                throw new InvalidOperationException("Invalid click state: " + _clickState);
        }

        switch (_rightClickState)
        {
            case RightClickState.None:
                if (_mouseIsOver && Mouse.RightPressed)
                {
                    _rightClickState = RightClickState.DownInside;
                    RightPressed?.Invoke();
                }
                break;

            case RightClickState.DownInside:
                if (!_mouseIsOver)
                {
                    if (Mouse.RightReleased)
                    {
                        _rightClickState = RightClickState.None;
                    }
                    else
                    {
                        _rightClickState = RightClickState.DownOutside;
                    }
                }
                else if (Mouse.RightReleased)
                {
                    _rightClickState = RightClickState.None;
                    RightClicked?.Invoke();
                }
                break;

            case RightClickState.DownOutside:
                if (Mouse.RightReleased)
                {
                    _rightClickState = RightClickState.None;
                }
                else if (_mouseIsOver)
                {
                    _rightClickState = RightClickState.DownInside;
                }
                break;

            default:
                throw new InvalidOperationException("Invalid right click state: " + _rightClickState);
        }

        if (_mouseIsOver && InterceptsMouseScroll && Mouse.WheelDelta != 0f)
        {
            ScrollWheelMoved?.Invoke(Mouse.WheelDelta);
            MouseSolver.ConsumeWheelScroll();
        }
    }

    private static bool IsDetached(IMouseInteractable target)
    {
        for (IAddable? cursor = target as IAddable; cursor is not null; cursor = cursor.Parent as IAddable)
        {
            if (cursor.Parent is null)
            {
                return true;
            }
        }
        return false;
    }
}
