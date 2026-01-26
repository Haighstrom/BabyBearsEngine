using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Tests.System.Source.BearSpinner3000;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        AddEntity(new BearSpinnerButton(20, 20));

        var texture = new TextureFactory().CreateTextureFromImageFile("Assets/bear.png");
        AddGraphic(new Image(texture, 200, 100, 300, 300) { Angle = 45f });
    }
}
