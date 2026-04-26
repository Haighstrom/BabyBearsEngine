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

        //work on the basis that the top-most entity will be the last one to register itself as moused over
        var topMostClicked = s_currentMousedOver.Count > 0 ? s_currentMousedOver[^1] : null;

        topMostClicked?.SetMouseOver(true);

        s_prevMousedOver = [.. s_currentMousedOver];
        s_currentMousedOver.Clear();
    }
}
