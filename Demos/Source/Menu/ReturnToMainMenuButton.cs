using System;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class ReturnToMainMenuButton(Func<World> menuWorldFactory)
    : Button(5, 5, 80, 40, Colour.Yellow, "Return")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();
        Engine.ChangeWorld(menuWorldFactory());
    }
}
