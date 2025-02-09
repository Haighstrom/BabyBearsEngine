using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Input;

public static class Mouse
{
    private static GameWindow? s_window;
    private static MouseState? s_currentState;
    private static MouseState? s_previousState;

    internal static void Initialise(GameWindow window)
    {
        s_window = window;
        s_currentState = s_previousState = window.MouseState;
    }

    internal static void Update()
    {
        if (s_window is null)
        {
            throw new InvalidOperationException("Mouse has not been initialised.");
        }

        s_previousState = s_currentState;
        s_currentState = s_window.MouseState;
    }

    public static bool ButtonDown(MouseButton button)
    {
        if (s_currentState is null)
        {
            throw new InvalidOperationException("Mouse has not been initialised.");
        }

        return s_currentState.IsButtonDown(button);
    }

    public static bool ButtonPressed(MouseButton button)
    {
        if (s_currentState is null || s_previousState is null)
        {
            throw new InvalidOperationException("Mouse has not been initialised.");
        }

        return s_currentState.IsButtonDown(button) && !s_previousState.IsButtonDown(button);
    }

    public static bool ButtonReleased(MouseButton button)
    {
        if (s_currentState is null || s_previousState is null)
        {
            throw new InvalidOperationException("Mouse has not been initialised.");
        }

        return !s_currentState.IsButtonDown(button) && s_previousState.IsButtonDown(button);
    }

    public static bool AnyButtonDown(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonDown);

    public static bool AnyButtonDown(params MouseButton[] buttons) => buttons.Any(ButtonDown);

    public static bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonPressed);

    public static bool AnyButtonPressed(params MouseButton[] buttons) => buttons.Any(ButtonPressed);

    public static bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonReleased);

    public static bool AnyButtonReleased(params MouseButton[] buttons) => buttons.Any(ButtonReleased);

    public static bool AllButtonsDown(IEnumerable<MouseButton> buttons) => buttons.All(ButtonDown);

    public static bool AllButtonsDown(params MouseButton[] buttons) => buttons.All(ButtonDown);

    public static bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => buttons.All(ButtonPressed);

    public static bool AllButtonsPressed(params MouseButton[] buttons) => buttons.All(ButtonPressed);

    public static bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => buttons.All(ButtonReleased);

    public static bool AllButtonsReleased(params MouseButton[] buttons) => buttons.All(ButtonReleased);

    public static bool LeftDown => ButtonDown(MouseButton.Left);
    public static bool MiddleDown => ButtonDown(MouseButton.Middle);
    public static bool RightDown => ButtonDown(MouseButton.Right);
    public static bool LeftUp => !ButtonDown(MouseButton.Left);
    public static bool MiddleUp => !ButtonDown(MouseButton.Middle);
    public static bool RightUp => !ButtonDown(MouseButton.Right);
    public static bool LeftPressed => ButtonPressed(MouseButton.Left);
    public static bool MiddlePressed => ButtonPressed(MouseButton.Middle);
    public static bool RightPressed => ButtonPressed(MouseButton.Right);
    public static bool LeftReleased => ButtonReleased(MouseButton.Left);
    public static bool MiddleReleased => ButtonReleased(MouseButton.Middle);
    public static bool RightReleased => ButtonReleased(MouseButton.Right);

    public static int ClientX => (int)(s_currentState?.X ?? throw new InvalidOperationException("Mouse has not been initialised."));
    public static int ClientY => (int)(s_currentState?.Y ?? throw new InvalidOperationException("Mouse has not been initialised."));
    public static float WheelDelta => s_currentState?.ScrollDelta.Y ?? throw new InvalidOperationException("Mouse has not been initialised.");
    public static int XDelta => (int)(s_currentState?.Delta.X ?? throw new InvalidOperationException("Mouse has not been initialised."));
    public static int YDelta => (int)(s_currentState?.Delta.Y ?? throw new InvalidOperationException("Mouse has not been initialised."));
}
