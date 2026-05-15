using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Sandbox;

internal class ScratchWorld : World
{
    public ScratchWorld()
    {
        var msaa = MsaaSamples.X2;

        var texture = Textures.CreateFromFile("Assets/bear.png");
        Add(new TextureGraphic(texture, 200, 100, 300, 300) { Angle = 45f });

        var camera = Camera.WithTileSize(500, 50, 100, 100, 10, 10, msaa);
        camera.Add(new ColourGraphic(Colour.Yellow, 0, 0, 5, 5));

        var newBearTex = Textures.CreateFromFile("Assets/bear.png");
        camera.Add(new TextureGraphic(newBearTex, 5, 5, 3, 3) { Angle = 45 });

        Add(camera);
    }
}
