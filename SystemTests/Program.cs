using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Worlds;

using (HaighWindow game = new(800, 600, "Bears"))
{
    World world = new();

    world.AddGraphic(new Image("Assets/bear.jpg", 50, 50, 200, 200));
    world.AddGraphic(new Image("Assets/bird.jpg", 450, 50, 200, 200));
    world.AddGraphic(new Image("Assets/fish.jpg", 50, 350, 200, 200));
    world.AddGraphic(new Image("Assets/lizard.jpg", 450, 350, 200, 200));

    game.World = world;

    game.Run();
}
