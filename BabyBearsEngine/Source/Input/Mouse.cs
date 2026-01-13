using System.Collections.Generic;
using BabyBearsEngine.Runtime;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Input;

internal static class Mouse
{
    private static IMouseService Service => RuntimeServices.MouseService;

    public static bool ButtonDown(MouseButton button) => Service.ButtonDown(button);
    public static bool ButtonPressed(MouseButton button) => Service.ButtonPressed(button);
    public static bool ButtonReleased(MouseButton button) => Service.ButtonReleased(button);

    public static bool AnyButtonDown(IEnumerable<MouseButton> buttons) => Service.AnyButtonDown(buttons);
    public static bool AnyButtonDown(params MouseButton[] buttons) => Service.AnyButtonDown(buttons);

    public static bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => Service.AnyButtonPressed(buttons);
    public static bool AnyButtonPressed(params MouseButton[] buttons) => Service.AnyButtonPressed(buttons);

    public static bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => Service.AnyButtonReleased(buttons);
    public static bool AnyButtonReleased(params MouseButton[] buttons) => Service.AnyButtonReleased(buttons);

    public static bool AllButtonsDown(IEnumerable<MouseButton> buttons) => Service.AllButtonsDown(buttons);
    public static bool AllButtonsDown(params MouseButton[] buttons) => Service.AllButtonsDown(buttons);

    public static bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => Service.AllButtonsPressed(buttons);
    public static bool AllButtonsPressed(params MouseButton[] buttons) => Service.AllButtonsPressed(buttons);

    public static bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => Service.AllButtonsReleased(buttons);
    public static bool AllButtonsReleased(params MouseButton[] buttons) => Service.AllButtonsReleased(buttons);

    public static bool LeftDown => Service.LeftDown;
    public static bool MiddleDown => Service.MiddleDown;
    public static bool RightDown => Service.RightDown;

    public static bool LeftUp => Service.LeftUp;
    public static bool MiddleUp => Service.MiddleUp;
    public static bool RightUp => Service.RightUp;

    public static bool LeftPressed => Service.LeftPressed;
    public static bool MiddlePressed => Service.MiddlePressed;
    public static bool RightPressed => Service.RightPressed;

    public static bool LeftReleased => Service.LeftReleased;
    public static bool MiddleReleased => Service.MiddleReleased;
    public static bool RightReleased => Service.RightReleased;

    public static int ClientX => Service.ClientX;
    public static int ClientY => Service.ClientY;

    public static float WheelDelta => Service.WheelDelta;
    public static int XDelta => Service.XDelta;
    public static int YDelta => Service.YDelta;
}

