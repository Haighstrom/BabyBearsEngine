using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Input;

public static class Mouse
{
    private static MouseState? s_mouseState;

    internal static void Initialise(MouseState mouseState)
    {
        s_mouseState = mouseState;
    }

    internal static void Update()
    {
        Ensure.NotNull(s_mouseState);

        if (s_mouseState.IsButtonPressed(MouseButton.Button1))
        {
            Console.WriteLine("Mouse.cs: Mouse Button 1 is down");
        }
    }

    public static bool ButtonDown(MouseButton button)
    {
        Ensure.NotNull(s_mouseState);

        return s_mouseState.IsButtonDown(button);
    }

    public static bool ButtonPressed(MouseButton button)
    {
        Ensure.NotNull(s_mouseState);

        return s_mouseState.IsButtonPressed(button);
    }

    public static bool ButtonReleased(MouseButton button)
    {
        Ensure.NotNull(s_mouseState);

        return s_mouseState.IsButtonReleased(button);
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

    public static int ClientX => (int)(s_mouseState?.X ?? throw new InvalidOperationException("Mouse has not been initialised."));
    public static int ClientY => (int)(s_mouseState?.Y ?? throw new InvalidOperationException("Mouse has not been initialised."));
    public static float WheelDelta => s_mouseState?.ScrollDelta.Y ?? throw new InvalidOperationException("Mouse has not been initialised.");
    public static int XDelta => (int)(s_mouseState?.Delta.X ?? throw new InvalidOperationException("Mouse has not been initialised."));
    public static int YDelta => (int)(s_mouseState?.Delta.Y ?? throw new InvalidOperationException("Mouse has not been initialised."));
}
