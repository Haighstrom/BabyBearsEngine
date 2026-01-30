namespace BabyBearsEngine.Platform.OpenTK;

using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Input;
using global::OpenTK.Windowing.GraphicsLibraryFramework;

internal sealed class OpenTKMouseAdapter(MouseState mouseState) : IMouse
{
    public bool ButtonDown(MouseButton button) => mouseState.IsButtonDown(button);
    public bool ButtonPressed(MouseButton button) => mouseState.IsButtonPressed(button);
    public bool ButtonReleased(MouseButton button) => mouseState.IsButtonReleased(button);

    public bool AnyButtonDown(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonDown);
    public bool AnyButtonDown(params MouseButton[] buttons) => buttons.Any(ButtonDown);

    public bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonPressed);
    public bool AnyButtonPressed(params MouseButton[] buttons) => buttons.Any(ButtonPressed);

    public bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonReleased);
    public bool AnyButtonReleased(params MouseButton[] buttons) => buttons.Any(ButtonReleased);

    public bool AllButtonsDown(IEnumerable<MouseButton> buttons) => buttons.All(ButtonDown);
    public bool AllButtonsDown(params MouseButton[] buttons) => buttons.All(ButtonDown);

    public bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => buttons.All(ButtonPressed);
    public bool AllButtonsPressed(params MouseButton[] buttons) => buttons.All(ButtonPressed);

    public bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => buttons.All(ButtonReleased);
    public bool AllButtonsReleased(params MouseButton[] buttons) => buttons.All(ButtonReleased);

    public bool LeftDown => ButtonDown(MouseButton.Left);
    public bool MiddleDown => ButtonDown(MouseButton.Middle);
    public bool RightDown => ButtonDown(MouseButton.Right);

    public bool LeftUp => !LeftDown;
    public bool MiddleUp => !MiddleDown;
    public bool RightUp => !RightDown;

    public bool LeftPressed => ButtonPressed(MouseButton.Left);
    public bool MiddlePressed => ButtonPressed(MouseButton.Middle);
    public bool RightPressed => ButtonPressed(MouseButton.Right);

    public bool LeftReleased => ButtonReleased(MouseButton.Left);
    public bool MiddleReleased => ButtonReleased(MouseButton.Middle);
    public bool RightReleased => ButtonReleased(MouseButton.Right);

    public int ClientX => (int)mouseState.X;
    public int ClientY => (int)mouseState.Y;

    public float WheelDelta => mouseState.ScrollDelta.Y;
    public int XDelta => (int)mouseState.Delta.X;
    public int YDelta => (int)mouseState.Delta.Y;
}

