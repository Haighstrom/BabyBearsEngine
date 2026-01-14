using BabyBearsEngine.Source.Worlds.UI;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class ReturnToMainMenuButton()
    : Button(10, 10, 80, 40, Color4.Yellow, "Return")
{
    public override void OnClicked()
    {
        Window.ChangeWorld(new MenuWorld());
    }
}
