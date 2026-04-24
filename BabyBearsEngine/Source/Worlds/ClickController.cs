using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Source.Worlds;

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
/// <see cref="LeftReleased"/> is only raised when the mouse was pressed while over the
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

    private ClickState _clickState = ClickState.None;
    private double _hoverTimeElapsed = 0;
    private bool _mouseIsOver = false;

    public bool Active { get; set; } = true;

    public event Action? HoverCancelled;
    public event Action? Hovered;
    public event Action? LeftPressed;
    public event Action? LeftReleased;
    public event Action? MouseEntered;
    public event Action? MouseExited;

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
        if (target.PositionOnScreen.Contains(Mouse.ClientX, Mouse.ClientY))
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

                    if (_hoverTimeElapsed >= timeToTriggerHover)
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
                if (Mouse.LeftReleased)
                {
                    _clickState = ClickState.MouseOver;
                    _hoverTimeElapsed = 0;
                    LeftReleased?.Invoke();
                }
                else if (!_mouseIsOver)
                {
                    _clickState = ClickState.MouseDownOutside;
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
    }
}
