using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        var msaa = MsaaSamples.X2;

        Add(new BearSpinnerButton(20, 20));
        Add(new TextDemoButton(20, 85));

        var texture = Textures.CreateFromFile("Assets/bear.png");

        Add(new Image(texture, 200, 100, 300, 300) { Angle = 45f });

        var camera = new Camera(500, 50, 100, 100, 10, 10, msaa);
        var rect = new ColouredRectangle(Colour.Yellow, 0, 0, 5, 5);

        var newBearTex = Textures.CreateFromFile("Assets/bear.png");
        camera.Add(new Image(newBearTex, 5, 5, 3, 3) { Angle = 45 });

        camera.Add(rect);
        Add(camera);
    }
}
