using System.Collections.Generic;

namespace BabyBearsEngine.Input;

/// <summary>
/// Per-frame mouse input snapshot. Methods report state as of the current frame:
/// <c>...Down</c>/<c>...Up</c> describe the current button state; <c>...Pressed</c> and
/// <c>...Released</c> detect the single frame of a state change (edge-triggered).
/// Position is reported in client (window) coordinates with the origin at the top-left.
/// </summary>
public interface IMouse
{
    /// <summary>Returns true if <paramref name="button"/> is currently held down.</summary>
    bool ButtonDown(MouseButton button);

    /// <summary>Returns true on the single frame that <paramref name="button"/> transitioned from up to down.</summary>
    bool ButtonPressed(MouseButton button);

    /// <summary>Returns true on the single frame that <paramref name="button"/> transitioned from down to up.</summary>
    bool ButtonReleased(MouseButton button);

    /// <summary>Returns true if any button in <paramref name="buttons"/> is currently held down.</summary>
    bool AnyButtonDown(IEnumerable<MouseButton> buttons);

    /// <summary>Returns true if any button in <paramref name="buttons"/> is currently held down.</summary>
    bool AnyButtonDown(params MouseButton[] buttons);

    /// <summary>Returns true if any button in <paramref name="buttons"/> was pressed this frame.</summary>
    bool AnyButtonPressed(IEnumerable<MouseButton> buttons);

    /// <summary>Returns true if any button in <paramref name="buttons"/> was pressed this frame.</summary>
    bool AnyButtonPressed(params MouseButton[] buttons);

    /// <summary>Returns true if any button in <paramref name="buttons"/> was released this frame.</summary>
    bool AnyButtonReleased(IEnumerable<MouseButton> buttons);

    /// <summary>Returns true if any button in <paramref name="buttons"/> was released this frame.</summary>
    bool AnyButtonReleased(params MouseButton[] buttons);

    /// <summary>Returns true only if every button in <paramref name="buttons"/> is currently held down.</summary>
    bool AllButtonsDown(IEnumerable<MouseButton> buttons);

    /// <summary>Returns true only if every button in <paramref name="buttons"/> is currently held down.</summary>
    bool AllButtonsDown(params MouseButton[] buttons);

    /// <summary>Returns true only if every button in <paramref name="buttons"/> was pressed this frame.</summary>
    bool AllButtonsPressed(IEnumerable<MouseButton> buttons);

    /// <summary>Returns true only if every button in <paramref name="buttons"/> was pressed this frame.</summary>
    bool AllButtonsPressed(params MouseButton[] buttons);

    /// <summary>Returns true only if every button in <paramref name="buttons"/> was released this frame.</summary>
    bool AllButtonsReleased(IEnumerable<MouseButton> buttons);

    /// <summary>Returns true only if every button in <paramref name="buttons"/> was released this frame.</summary>
    bool AllButtonsReleased(params MouseButton[] buttons);

    /// <summary>True while the left button is held down.</summary>
    bool LeftDown { get; }

    /// <summary>True while the middle button is held down.</summary>
    bool MiddleDown { get; }

    /// <summary>True while the right button is held down.</summary>
    bool RightDown { get; }

    /// <summary>True while the left button is not held down.</summary>
    bool LeftUp { get; }

    /// <summary>True while the middle button is not held down.</summary>
    bool MiddleUp { get; }

    /// <summary>True while the right button is not held down.</summary>
    bool RightUp { get; }

    /// <summary>True on the single frame the left button transitioned from up to down.</summary>
    bool LeftPressed { get; }

    /// <summary>True on the single frame the middle button transitioned from up to down.</summary>
    bool MiddlePressed { get; }

    /// <summary>True on the single frame the right button transitioned from up to down.</summary>
    bool RightPressed { get; }

    /// <summary>True on the single frame the left button transitioned from down to up.</summary>
    bool LeftReleased { get; }

    /// <summary>True on the single frame the middle button transitioned from down to up.</summary>
    bool MiddleReleased { get; }

    /// <summary>True on the single frame the right button transitioned from down to up.</summary>
    bool RightReleased { get; }

    /// <summary>The cursor's X coordinate in client (window) space, measured from the left edge.</summary>
    int ClientX { get; }

    /// <summary>The cursor's Y coordinate in client (window) space, measured from the top edge.</summary>
    int ClientY { get; }

    /// <summary>Vertical scroll wheel movement this frame. Positive values scroll up, negative down.</summary>
    float WheelDelta { get; }

    /// <summary>Cursor X movement this frame, in pixels.</summary>
    int XDelta { get; }

    /// <summary>Cursor Y movement this frame, in pixels.</summary>
    int YDelta { get; }
}
