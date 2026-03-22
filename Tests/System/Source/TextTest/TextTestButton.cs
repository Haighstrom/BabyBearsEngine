namespace BabyBearsEngine.Tests.System.Source.TextTest;

internal class TextTestButton(int x, int y)
    : Button(x, y, 120, 60, Colour.White, "Text Test")
{
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();

        Engine.ChangeWorld(new TextTestWorld());
    }
}
