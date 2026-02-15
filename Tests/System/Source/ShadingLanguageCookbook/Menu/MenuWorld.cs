using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Tests.System.Source.BearSpinner3000;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class MenuWorld : World
{
    ColouredRectangle rect;
    public MenuWorld()
    {
        Add(new BearSpinnerButton(20, 20));

        var texture = new TextureFactory().CreateTextureFromImageFile("Assets/bear.png");
        Add(new Image(texture, 200, 100, 300, 300) { Angle = 45f });

        var camera = new Camera(0, 500, 50, 100, 100, 1, 1);
        rect = new ColouredRectangle(Colour.Yellow, 0, 0, 50, 50);
        camera.Add(rect);
        Add(camera);
    }

    public override void Draw()
    {
        rect.TempShaderResize(100,100);
        base.Draw();
    }
}
