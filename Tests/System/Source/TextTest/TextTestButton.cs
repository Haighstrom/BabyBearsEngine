namespace BabyBearsEngine.Tests.System.Source.TextTest;

internal class TextTestButton(int x, int y)
    : Button(x, y, 120, 60, Colour.White, "Text Test")
{
    public override void OnClicked()
    {
        base.OnClicked();

        Engine.ChangeWorld(new TextTestWorld());
    }
}
