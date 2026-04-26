namespace BabyBearsEngine.Demos.Source.ShadingLanguageCookbook.Menu;

internal class ReturnToMainMenuButton()
    : Button(10, 10, 80, 40, Colour.Yellow, "Return")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        Engine.ChangeWorld(new MenuWorld());
    }
}
