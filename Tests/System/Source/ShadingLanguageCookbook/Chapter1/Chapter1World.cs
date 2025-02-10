using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;

internal class Chapter1World : World
{
    public Chapter1World(HaighWindow haighWindow, ShaderProgramLibrary shaderLibrary)
    {
        AddGraphic(new Triangle());
        AddEntity(new ReturnToMainMenuButton(haighWindow, shaderLibrary));
    }
}
