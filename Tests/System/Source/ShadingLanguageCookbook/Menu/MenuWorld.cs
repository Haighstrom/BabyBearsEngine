using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Text;
using BabyBearsEngine.Source.UI;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Tests.System.Source.BearSpinner3000;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        AddEntity(new BearSpinnerButton(20, 20));
    }
}
