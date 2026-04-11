using BabyBearsEngine.Input;

namespace BabyBearsEngine.Source.Worlds;

/// <summary>
/// Controls mouse interactions for a single <see cref="IClickable"/> target.
///
/// This class implements a small state machine to handle enter/exit, hover,
/// mouse down/up and the case where the user presses inside then drags
/// outside while holding the button.
///
/// <para>Notes:</para>
/// <para>The owner must call <see cref="SetMouseOver(bool)"/> each frame
/// <c>before</c> calling <see cref="Update(double)"/> so the controller sees
/// the current mouse-over state.</para>
/// <para>The controller assumes <c>Mouse.LeftPressed</c> and
/// <c>Mouse.LeftReleased</c> are edge events (true only on the frame the action
/// occurred).</para>
/// <para>
/// <c>Mouse.LeftReleased</c> events are only forwarded to the target when the
/// mouse was pressed while over the target and then released while still over
/// the target (this represents a successful "click"). If the mouse is pressed
/// over the target, dragged outside while the button remains down, and then
/// released outside, the controller emits <c>TriggerMouseExited()</c> instead
/// of <c>TriggerLeftReleased()</c> to indicate the interaction was cancelled.
/// </para>
/// <param name="clickTarget">The <see cref="IClickable"/> instance that will
/// receive the triggered events (entered/exit/hover/press/release). This must
/// not be null.</param>
/// <param name="timeToTriggerHover">Time in seconds the mouse must remain
/// over the target before a hover is triggered. Must be non-negative.</param>
/// </summary>
internal sealed class ClickController(IClickable clickTarget, double timeToTriggerHover = 0.5) : IClickController
{
    // State machine describing the click/hover lifecycle for the target.
    private enum ClickState
    {
        None,
        MouseOver,
        Hovering,
        MouseDownInside,
        MouseDownOutside,
    }

    // Set by caller via SetMouseOver; indicates whether the mouse is currently
    // over the clickable target for this frame.
    private bool _mouseIsOver = false;

    // Current state of the controller.
    private ClickState _clickState = ClickState.None;

    // Accumulated time spent in the MouseOver state; used to detect hover.
    private double _hoverTimeElapsed = 0;

    /// <summary>
    /// Inform the controller whether the mouse is over the target this frame.
    /// Must be called before <see cref="Update(double)"/>.
    /// </summary>
    public void SetMouseOver(bool isMouseOver)
    {
        _mouseIsOver = isMouseOver;
    }

    /// <summary>
    /// Advance the controller state by <paramref name="elapsed"/> seconds.
    /// Handles Enter/Exit, Hover, Press and Release transitions and triggers
    /// the appropriate <see cref="IClickable"/> methods.
    /// </summary>
    public void Update(double elapsed)
    {
        switch (_clickState)
        {
            case ClickState.None:
                // No mouse over and nothing happening. If the mouse now becomes
                // over the target, enter MouseOver and fire MouseEntered.
                if (_mouseIsOver)
                {
                    _clickState = ClickState.MouseOver;
                    _hoverTimeElapsed = 0; // start hover timer when becoming mouse-over
                    clickTarget.TriggerMouseEntered();

                    // If the left button was pressed on the same frame we entered
                    // mouse-over, immediately transition to MouseDownInside and
                    // fire the left-pressed event so the click is not lost.
                    if (Mouse.LeftPressed)
                    {
                        _clickState = ClickState.MouseDownInside;
                        clickTarget.TriggerLeftPressed();
                    }
                }
                break;

            case ClickState.MouseOver:
                // While the mouse is over the target we either detect an exit,
                // a press, or we advance the hover timer until Hovering.
                if (!_mouseIsOver)
                {
                    _clickState = ClickState.None;
                    clickTarget.TriggerMouseExited();
                }
                else if (Mouse.LeftPressed)
                {
                    _clickState = ClickState.MouseDownInside;
                    clickTarget.TriggerLeftPressed();
                }
                else
                {
                    _hoverTimeElapsed += elapsed;

                    if (_hoverTimeElapsed >= timeToTriggerHover)
                    {
                        _clickState = ClickState.Hovering;
                        clickTarget.TriggerHover();
                    }
                }
                break;

            case ClickState.Hovering:
                // While hovering, a mouse exit or a press cancels the hover. A
                // press transitions to MouseDownInside and should also trigger
                // the press event (so clicks while hovering are detected).
                if (!_mouseIsOver)
                {
                    _clickState = ClickState.None;
                    clickTarget.TriggerCancelHover();
                    clickTarget.TriggerMouseExited();
                }
                else if (Mouse.LeftPressed)
                {
                    _clickState = ClickState.MouseDownInside;
                    clickTarget.TriggerCancelHover();
                    clickTarget.TriggerLeftPressed();
                }
                break;

            case ClickState.MouseDownInside:
                // Button was pressed while mouse was over the target. If the
                // button is released while still over the target we fire
                // LeftReleased and return to MouseOver. If the mouse is moved
                // outside while holding the button we enter MouseDownOutside
                // and delay exit until the release.
                if (Mouse.LeftReleased)
                {
                    _clickState = ClickState.MouseOver;
                    _hoverTimeElapsed = 0; // restart hover timing after click
                    clickTarget.TriggerLeftReleased();
                }
                else if (!_mouseIsOver)
                {
                    _clickState = ClickState.MouseDownOutside;
                    // delay triggering mouse exit until mouse is released
                }
                break;

            case ClickState.MouseDownOutside:
                // Button was pressed inside then the cursor moved outside while
                // holding the button. When released we emit MouseExited (a
                // cancellation) and return to None. If the cursor moves back
                // inside while still pressed we resume MouseDownInside.
                if (Mouse.LeftReleased)
                {
                    _clickState = ClickState.None;
                    clickTarget.TriggerMouseExited(); // delayed exit on release
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
