namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class ReturnToMainMenuButton()
    : Button(10, 10, 80, 40, Colour.Yellow, "Return")
{
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();

        Engine.ChangeWorld(new MenuWorld());
    }
}
