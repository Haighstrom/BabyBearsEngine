using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Tests.System.Source.BearSpinner3000;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        AddEntity(new BearSpinnerButton(20, 20));

        //AddGraphic(new Image("Assets/bear.png", 20, 20, 300, 300) { Angle = 45f });
    }
}
