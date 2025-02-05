using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;
using OpenTK.Mathematics;

using (HaighWindow game = new(800, 600, "Bears"))
{
    //ConsoleWindow.Open();

    //World world = new();

    //var shaderLibrary = new ShaderProgramLibrary(game);

    //world.AddGraphic(new Image(shaderLibrary, "Assets/bear.jpg", 0, 0, 400, 300));
    //world.AddGraphic(new PointGraphic(shaderLibrary, 600, 100, 30, Color4.Red));

    World world = new Chapter1World();


    game.World = world;

    game.Run();
}
