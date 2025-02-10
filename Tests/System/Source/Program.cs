using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Debugging;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

using (HaighWindow window = new(800, 600, "Bears"))
{
    ConsoleWindow.Open();

    var shaderLibrary = new ShaderProgramLibrary(window);

    World world = new Chapter1World(window, shaderLibrary);

    window.World = world;

    window.Run();
}
