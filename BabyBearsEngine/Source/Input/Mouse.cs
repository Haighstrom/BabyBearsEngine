namespace BabyBearsEngine.Input;

/// <summary>
/// Static facade over the installed <see cref="IMouse"/> service. All members route through
/// <c>EngineConfiguration.MouseService</c>; tests substitute a fake there to exercise consumers
/// without a real input device. Throws <see cref="InvalidOperationException"/> if accessed before the engine is initialised.
/// </summary>
/// <remarks>
/// The engine supports a single window per process, so there is one shared mouse state; running
/// two games concurrently in the same process is not supported.
/// </remarks>
public static class Mouse
{
    private static IMouse Implementation => EngineConfiguration.MouseService;

    /// <inheritdoc cref="IMouse.ButtonDown(MouseButton)"/>
    public static bool ButtonDown(MouseButton button) => Implementation.ButtonDown(button);

    /// <inheritdoc cref="IMouse.ButtonPressed(MouseButton)"/>
    public static bool ButtonPressed(MouseButton button) => Implementation.ButtonPressed(button);

    /// <inheritdoc cref="IMouse.ButtonReleased(MouseButton)"/>
    public static bool ButtonReleased(MouseButton button) => Implementation.ButtonReleased(button);

    /// <inheritdoc cref="IMouse.AnyButtonDown(IEnumerable{MouseButton})"/>
    public static bool AnyButtonDown(IEnumerable<MouseButton> buttons) => Implementation.AnyButtonDown(buttons);

    /// <inheritdoc cref="IMouse.AnyButtonDown(MouseButton[])"/>
    public static bool AnyButtonDown(params MouseButton[] buttons) => Implementation.AnyButtonDown(buttons);

    /// <inheritdoc cref="IMouse.AnyButtonPressed(IEnumerable{MouseButton})"/>
    public static bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => Implementation.AnyButtonPressed(buttons);

    /// <inheritdoc cref="IMouse.AnyButtonPressed(MouseButton[])"/>
    public static bool AnyButtonPressed(params MouseButton[] buttons) => Implementation.AnyButtonPressed(buttons);

    /// <inheritdoc cref="IMouse.AnyButtonReleased(IEnumerable{MouseButton})"/>
    public static bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => Implementation.AnyButtonReleased(buttons);

    /// <inheritdoc cref="IMouse.AnyButtonReleased(MouseButton[])"/>
    public static bool AnyButtonReleased(params MouseButton[] buttons) => Implementation.AnyButtonReleased(buttons);

    /// <inheritdoc cref="IMouse.AllButtonsDown(IEnumerable{MouseButton})"/>
    public static bool AllButtonsDown(IEnumerable<MouseButton> buttons) => Implementation.AllButtonsDown(buttons);

    /// <inheritdoc cref="IMouse.AllButtonsDown(MouseButton[])"/>
    public static bool AllButtonsDown(params MouseButton[] buttons) => Implementation.AllButtonsDown(buttons);

    /// <inheritdoc cref="IMouse.AllButtonsPressed(IEnumerable{MouseButton})"/>
    public static bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => Implementation.AllButtonsPressed(buttons);

    /// <inheritdoc cref="IMouse.AllButtonsPressed(MouseButton[])"/>
    public static bool AllButtonsPressed(params MouseButton[] buttons) => Implementation.AllButtonsPressed(buttons);

    /// <inheritdoc cref="IMouse.AllButtonsReleased(IEnumerable{MouseButton})"/>
    public static bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => Implementation.AllButtonsReleased(buttons);

    /// <inheritdoc cref="IMouse.AllButtonsReleased(MouseButton[])"/>
    public static bool AllButtonsReleased(params MouseButton[] buttons) => Implementation.AllButtonsReleased(buttons);

    /// <inheritdoc cref="IMouse.LeftDown"/>
    public static bool LeftDown => Implementation.LeftDown;

    /// <inheritdoc cref="IMouse.MiddleDown"/>
    public static bool MiddleDown => Implementation.MiddleDown;

    /// <inheritdoc cref="IMouse.RightDown"/>
    public static bool RightDown => Implementation.RightDown;

    /// <inheritdoc cref="IMouse.LeftUp"/>
    public static bool LeftUp => Implementation.LeftUp;

    /// <inheritdoc cref="IMouse.MiddleUp"/>
    public static bool MiddleUp => Implementation.MiddleUp;

    /// <inheritdoc cref="IMouse.RightUp"/>
    public static bool RightUp => Implementation.RightUp;

    /// <inheritdoc cref="IMouse.LeftPressed"/>
    public static bool LeftPressed => Implementation.LeftPressed;

    /// <inheritdoc cref="IMouse.MiddlePressed"/>
    public static bool MiddlePressed => Implementation.MiddlePressed;

    /// <inheritdoc cref="IMouse.RightPressed"/>
    public static bool RightPressed => Implementation.RightPressed;

    /// <inheritdoc cref="IMouse.LeftReleased"/>
    public static bool LeftReleased => Implementation.LeftReleased;

    /// <inheritdoc cref="IMouse.MiddleReleased"/>
    public static bool MiddleReleased => Implementation.MiddleReleased;

    /// <inheritdoc cref="IMouse.RightReleased"/>
    public static bool RightReleased => Implementation.RightReleased;

    /// <inheritdoc cref="IMouse.ClientX"/>
    public static int ClientX => Implementation.ClientX;

    /// <inheritdoc cref="IMouse.ClientY"/>
    public static int ClientY => Implementation.ClientY;

    /// <inheritdoc cref="IMouse.WheelDelta"/>
    public static float WheelDelta => Implementation.WheelDelta;

    /// <inheritdoc cref="IMouse.XDelta"/>
    public static int XDelta => Implementation.XDelta;

    /// <inheritdoc cref="IMouse.YDelta"/>
    public static int YDelta => Implementation.YDelta;
}
