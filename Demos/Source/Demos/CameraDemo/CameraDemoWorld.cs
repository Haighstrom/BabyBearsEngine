using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Rendering.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.CameraDemo;

internal class CameraDemoWorld : DemoWorld
{
    private const int CameraWidth = 390;
    private const int CameraHeight = 545;
    private const int BearCount = 8;

    public override string Name => "Camera Demo";

    public CameraDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        var fontDef = new FontDefinition("Times New Roman", 16);

        var leftCamera = Camera.WithTileSize(5, 50, CameraWidth, CameraHeight, 1, 1);
        leftCamera.BackgroundColour = Colour.AliceBlue;
        leftCamera.Add(new TextImage(fontDef, "No MSAA", Colour.Black, 5, 5, CameraWidth - 10, 30));

        var rightCamera = Camera.WithTileSize(405, 50, CameraWidth, CameraHeight, 1, 1, MsaaSamples.X4);
        rightCamera.BackgroundColour = Colour.Cornsilk;
        rightCamera.Add(new TextImage(fontDef, "MSAA x4", Colour.Black, 5, 5, CameraWidth - 10, 30));

        for (int i = 0; i < BearCount; i++)
        {
            leftCamera.Add(new BearEntity(Randomisation.Rand(CameraWidth), Randomisation.Rand(40, CameraHeight)));
            rightCamera.Add(new BearEntity(Randomisation.Rand(CameraWidth), Randomisation.Rand(40, CameraHeight)));
        }

        Add(leftCamera);
        Add(rightCamera);
    }
}
