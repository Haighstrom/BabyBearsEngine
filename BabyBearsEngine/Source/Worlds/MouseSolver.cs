using System.Collections.Generic;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

internal static class MouseSolver
{
    private static List<IClickController> s_prevMousedOver = [];
    private static readonly List<IClickController> s_currentMousedOver = [];

    // Controllers under the cursor at the moment the left button was first pressed.
    // While the button is held and this set is non-empty, only these controllers can
    // receive mouse-over state — preventing unrelated controls from highlighting or
    // receiving hover events while the user is dragging.
    private static List<IClickController> s_lockedSet = [];

    /// <summary>
    /// True when an entity with <see cref="ClickController.InterceptsMouseScroll"/> consumed the
    /// scroll wheel this frame. World-level scroll handlers (e.g. camera) should check this flag
    /// after calling <c>base.Update()</c> and skip their scroll logic when it is set.
    /// </summary>
    public static bool WheelScrollConsumed { get; private set; } = false;

    internal static void ConsumeWheelScroll() => WheelScrollConsumed = true;

    public static void RegisterMouseOver(IClickController clickController)
    {
        s_currentMousedOver.Add(clickController);
    }

    public static void Update()
    {
        // Capture the locked set on the frame the left button first goes down.
        if (Mouse.LeftPressed)
        {
            s_lockedSet = [.. s_currentMousedOver];
        }

        foreach (var prev in s_prevMousedOver)
        {
            prev.SetMouseOver(false);
        }

        bool locked = Mouse.LeftDown && s_lockedSet.Count > 0;

        // The top-most entity is the last to register. Walk downward through the stack,
        // notifying each controller until we reach one that does not pass mouse events through.
        // When locked, skip any controller that was not under the cursor when the button was pressed.
        for (int i = s_currentMousedOver.Count - 1; i >= 0; i--)
        {
            if (locked && !s_lockedSet.Contains(s_currentMousedOver[i]))
            {
                continue;
            }

            s_currentMousedOver[i].SetMouseOver(true);

            if (!s_currentMousedOver[i].ClickThrough)
            {
                break;
            }
        }

        if (!Mouse.LeftDown)
        {
            s_lockedSet = [];
        }

        s_prevMousedOver = [.. s_currentMousedOver];
        s_currentMousedOver.Clear();
        WheelScrollConsumed = false;
    }

    internal static void Reset()
    {
        s_prevMousedOver.Clear();
        s_currentMousedOver.Clear();
        s_lockedSet = [];
        WheelScrollConsumed = false;
    }
}
