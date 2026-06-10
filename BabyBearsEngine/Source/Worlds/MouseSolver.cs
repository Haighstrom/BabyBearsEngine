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
    private static HashSet<IClickController> s_lockedSet = [];

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

    /// <summary>
    /// Resolves mouse-over state for the frame: clears last frame's moused-over controllers, then
    /// notifies the top-most controller(s) currently under the cursor, honouring the drag lock set.
    /// </summary>
    /// <remarks>
    /// <strong>Ordering contract:</strong> call this once per frame <em>after</em> every
    /// <see cref="ClickController.Update"/> has run. Those calls populate the current moused-over set
    /// via <see cref="RegisterMouseOver"/>; both the lock-set capture (on the press frame) and the
    /// top-most resolution read that set, so running this before the controllers have registered
    /// would see an empty or stale set. The engine guarantees the order by calling it after
    /// <c>World.Update</c> in <c>OpenTKGameEngine.OnUpdateFrame</c>.
    /// </remarks>
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
