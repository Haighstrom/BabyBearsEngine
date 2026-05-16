using System;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class BackButton(float x, float y, Func<World> backFactory)
    : Button(x, y, 80, 40, ButtonTheme.FromColour(new Colour(210, 210, 215)), "← Back")
{
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();
        Engine.ChangeWorld(backFactory());
    }
}
