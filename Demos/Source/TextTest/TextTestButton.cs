namespace BabyBearsEngine.Demos.Source.TextTest;

internal class TextTestButton(int x, int y)
    : Button(x, y, 120, 60, Colour.White, "Text Test")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        Engine.ChangeWorld(new TextTestWorld());
    }
}
