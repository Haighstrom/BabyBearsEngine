namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class TextDemoButton(int x, int y)
    : Button(x, y, 120, 60, Colour.White, "Text Demo")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        Engine.ChangeWorld(new TextDemoWorld());
    }
}
