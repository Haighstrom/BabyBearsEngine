using System;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class DemoButton(int x, int y, string name, Func<World> factory)
    : Button(x, y, 120, 60, ButtonTheme.FromColour(new Colour(155, 210, 255)), name, multilineText: true)
{
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();
        Engine.ChangeWorld(factory());
    }
}
