using System.Collections.Generic;
using BabyBearsEngine.Runtime;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Input;

public static class Mouse
{
    private static IMouse Implementation => EngineConfiguration.MouseService;

    public static bool ButtonDown(MouseButton button) => Implementation.ButtonDown(button);
    public static bool ButtonPressed(MouseButton button) => Implementation.ButtonPressed(button);
    public static bool ButtonReleased(MouseButton button) => Implementation.ButtonReleased(button);

    public static bool AnyButtonDown(IEnumerable<MouseButton> buttons) => Implementation.AnyButtonDown(buttons);
    public static bool AnyButtonDown(params MouseButton[] buttons) => Implementation.AnyButtonDown(buttons);

    public static bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => Implementation.AnyButtonPressed(buttons);
    public static bool AnyButtonPressed(params MouseButton[] buttons) => Implementation.AnyButtonPressed(buttons);

    public static bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => Implementation.AnyButtonReleased(buttons);
    public static bool AnyButtonReleased(params MouseButton[] buttons) => Implementation.AnyButtonReleased(buttons);

    public static bool AllButtonsDown(IEnumerable<MouseButton> buttons) => Implementation.AllButtonsDown(buttons);
    public static bool AllButtonsDown(params MouseButton[] buttons) => Implementation.AllButtonsDown(buttons);

    public static bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => Implementation.AllButtonsPressed(buttons);
    public static bool AllButtonsPressed(params MouseButton[] buttons) => Implementation.AllButtonsPressed(buttons);

    public static bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => Implementation.AllButtonsReleased(buttons);
    public static bool AllButtonsReleased(params MouseButton[] buttons) => Implementation.AllButtonsReleased(buttons);

    public static bool LeftDown => Implementation.LeftDown;
    public static bool MiddleDown => Implementation.MiddleDown;
    public static bool RightDown => Implementation.RightDown;

    public static bool LeftUp => Implementation.LeftUp;
    public static bool MiddleUp => Implementation.MiddleUp;
    public static bool RightUp => Implementation.RightUp;

    public static bool LeftPressed => Implementation.LeftPressed;
    public static bool MiddlePressed => Implementation.MiddlePressed;
    public static bool RightPressed => Implementation.RightPressed;

    public static bool LeftReleased => Implementation.LeftReleased;
    public static bool MiddleReleased => Implementation.MiddleReleased;
    public static bool RightReleased => Implementation.RightReleased;

    public static int ClientX => Implementation.ClientX;
    public static int ClientY => Implementation.ClientY;

    public static float WheelDelta => Implementation.WheelDelta;
    public static int XDelta => Implementation.XDelta;
    public static int YDelta => Implementation.YDelta;
}

