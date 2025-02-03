using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Debugging;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Mathematics;

using (HaighWindow game = new(800, 600, "Bears"))
{
    World world = new();

    world.AddGraphic(new Image(game, "Assets/bear.jpg", 0, 0, 400, 300));
    world.AddGraphic(new PointGraphic(game, 600, 100, 30, Color4.Red));

    game.World = world;

    game.Run();
}
