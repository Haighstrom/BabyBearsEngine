using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Mathematics;

using (HaighWindow game = new(800, 600, "Bears"))
{
    World world = new();

    //ConsoleWindow.Open();
    
    var shaderLibrary = new ShaderProgramLibrary(game);

    world.AddGraphic(new Image(shaderLibrary, "Assets/bear.jpg", 0, 0, 400, 300));
    world.AddGraphic(new PointGraphic(shaderLibrary, 600, 100, 30, Color4.Red));



    game.World = world;

    game.Run();
}
