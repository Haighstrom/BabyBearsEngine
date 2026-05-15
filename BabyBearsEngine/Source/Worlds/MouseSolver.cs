using System.Collections.Generic;

namespace BabyBearsEngine.Worlds;

internal static class MouseSolver
{
    private static List<IClickController> s_prevMousedOver = [];
    private static readonly List<IClickController> s_currentMousedOver = [];

    public static void RegisterMouseOver(IClickController clickController)
    {
        s_currentMousedOver.Add(clickController);
    }

    public static void Update()
    {
        foreach (var prev in s_prevMousedOver)
        {
            prev.SetMouseOver(false);
        }

        // The top-most entity is the last to register. Walk downward through the stack,
        // notifying each controller until we reach one that does not pass mouse events through.
        for (int i = s_currentMousedOver.Count - 1; i >= 0; i--)
        {
            s_currentMousedOver[i].SetMouseOver(true);

            if (!s_currentMousedOver[i].ClickThrough)
            {
                break;
            }
        }

        s_prevMousedOver = [.. s_currentMousedOver];
        s_currentMousedOver.Clear();
    }

    internal static void Reset()
    {
        s_prevMousedOver.Clear();
        s_currentMousedOver.Clear();
    }
}
