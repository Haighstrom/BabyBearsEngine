using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Input;

public interface IMouseService
{
    bool ButtonDown(MouseButton button);
    bool ButtonPressed(MouseButton button);
    bool ButtonReleased(MouseButton button);

    bool AnyButtonDown(IEnumerable<MouseButton> buttons);
    bool AnyButtonDown(params MouseButton[] buttons);

    bool AnyButtonPressed(IEnumerable<MouseButton> buttons);
    bool AnyButtonPressed(params MouseButton[] buttons);

    bool AnyButtonReleased(IEnumerable<MouseButton> buttons);
    bool AnyButtonReleased(params MouseButton[] buttons);

    bool AllButtonsDown(IEnumerable<MouseButton> buttons);
    bool AllButtonsDown(params MouseButton[] buttons);

    bool AllButtonsPressed(IEnumerable<MouseButton> buttons);
    bool AllButtonsPressed(params MouseButton[] buttons);

    bool AllButtonsReleased(IEnumerable<MouseButton> buttons);
    bool AllButtonsReleased(params MouseButton[] buttons);

    bool LeftDown { get; }
    bool MiddleDown { get; }
    bool RightDown { get; }

    bool LeftUp { get; }
    bool MiddleUp { get; }
    bool RightUp { get; }

    bool LeftPressed { get; }
    bool MiddlePressed { get; }
    bool RightPressed { get; }

    bool LeftReleased { get; }
    bool MiddleReleased { get; }
    bool RightReleased { get; }

    int ClientX { get; }
    int ClientY { get; }

    float WheelDelta { get; }
    int XDelta { get; }
    int YDelta { get; }
}

