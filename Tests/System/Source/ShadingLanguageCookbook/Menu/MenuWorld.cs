using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        AddEntity(new OpenChapter1Button());
    }
}
