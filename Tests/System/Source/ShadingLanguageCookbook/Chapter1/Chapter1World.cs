using BabyBearsEngine.Source.Graphics.Text;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;

internal class Chapter1World : World
{
    public Chapter1World()
    {
        AddGraphic(new Triangle());
        AddEntity(new ReturnToMainMenuButton());
    }
}
