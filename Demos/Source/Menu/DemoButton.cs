using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class DemoButton(int x, int y, DemoWorld world)
    : Button(x, y, 120, 60, Colour.White, world.Name)
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();
        Engine.ChangeWorld(world);
    }
}
