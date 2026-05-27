using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.CameraDemo;

internal class CameraDemoWorld : DemoWorld
{
    private const int CamHeight = 547;
    private const int CamTop = 48;
    private const int CamWidth = 390;
    private const float TileH = 20f;
    private const float TileW = 10f;

    public override string Name => "Camera Demo";

    public CameraDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        FontDefinition labelFont = new("Times New Roman", 12);
        FontDefinition demoFont = new("Times New Roman", 20);

        Add(new TextGraphic(labelFont, "Without ScaleForCamera", Colour.Black, 5, 5, CamWidth, 22)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        Add(new TextGraphic(labelFont, "(text severely distorted)", new Colour(160, 80, 80), 5, 27, CamWidth, 18)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        Add(new TextGraphic(labelFont, "With ScaleForCamera(camera)", Colour.Black, 405, 5, CamWidth, 22)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        Add(new TextGraphic(labelFont, "(text at natural size)", new Colour(60, 130, 60), 405, 27, CamWidth, 18)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });

        Camera leftCamera = Camera.WithTileSize(5, CamTop, CamWidth, CamHeight, TileW, TileH);
        leftCamera.BackgroundColour = new Colour(255, 215, 215);
        leftCamera.Add(new TextGraphic(demoFont, "Hello!", Colour.Black, 0.5f, 0.5f, 20f, 5f));
        Add(leftCamera);

        Camera rightCamera = Camera.WithTileSize(405, CamTop, CamWidth, CamHeight, TileW, TileH);
        rightCamera.BackgroundColour = new Colour(215, 255, 215);
        TextGraphic rightText = new(demoFont,
            "Hello!\nText correctly\nscaled for camera.\nFont: 20pt\nTileSize: 10x20\nScaleForCamera applied.",
            Colour.Black, 0.5f, 0.5f, 38f, 26f)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
        };
        rightText.ScaleForCamera(rightCamera);
        rightCamera.Add(rightText);
        Add(rightCamera);
    }
}
