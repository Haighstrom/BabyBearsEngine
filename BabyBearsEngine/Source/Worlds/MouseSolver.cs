using System.Collections.Generic;

namespace BabyBearsEngine.Source.Worlds;

internal static class MouseSolver
{
    private static readonly List<ClickController> s_prevMousedOver = [];
    private static readonly List<ClickController> s_currentMousedOver = [];

    public static void RegisterMouseOver(ClickController clickController)
    {
        s_currentMousedOver.Add(clickController);
    }

    public static void Update()
    {
        //work on the basis that the top-most entity will be the last one to register itself as moused over
        var topMostClicked = s_currentMousedOver.Count > 0 ? s_currentMousedOver[^1] : null;

        topMostClicked?.SetMouseOver();

        s_currentMousedOver.Clear();
    }
}
