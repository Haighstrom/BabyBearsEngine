namespace BabyBearsEngine.Demos.Source.Demos.ClickDemo;

internal class ClickDemoButton(int x, int y)
    : Button(x, y, 120, 60, Colour.White, "Click Demo")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        Engine.ChangeWorld(new ClickDemoWorld());
    }
}
