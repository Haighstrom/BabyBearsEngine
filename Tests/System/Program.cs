using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Mathematics;

using (HaighWindow game = new(800, 600, "Bears"))
{
    World world = new();

    world.AddGraphic(new Image(game, "Assets/bear.jpg", 50, 50, 200, 200));
    world.AddGraphic(new ColouredRectangle(game, Color4.Orange, 450, 50, 200, 200));
    world.AddGraphic(new Image(game, "Assets/fish.jpg", 50, 350, 200, 200));
    world.AddGraphic(new Image(game, "Assets/lizard.jpg", 450, 350, 200, 200));

    game.World = world;

    game.Run();
}
