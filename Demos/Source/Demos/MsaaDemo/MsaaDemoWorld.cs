using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.MsaaDemo;

internal class MsaaDemoWorld : DemoWorld
{
    private const int CamHeight = 547;
    private const int CamTop = 48;
    private const int CamWidth = 390;

    public override string Name => "MSAA Demo";

    public MsaaDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        FontDefinition labelFont = new("Times New Roman", 12);

        Add(new TextGraphic(labelFont, "MSAA Disabled", Colour.Black, 5, 5, CamWidth, 22)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        Add(new TextGraphic(labelFont, "(jagged diagonal edges)", new Colour(160, 80, 80), 5, 27, CamWidth, 18)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        Add(new TextGraphic(labelFont, "MSAA x4", Colour.Black, 405, 5, CamWidth, 22)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        Add(new TextGraphic(labelFont, "(smooth diagonal edges)", new Colour(60, 130, 60), 405, 27, CamWidth, 18)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });

        var leftCamera = Camera.WithView(5, CamTop, CamWidth, CamHeight, 0, 0, CamWidth, CamHeight, samples: MsaaSamples.Disabled);
        leftCamera.BackgroundColour = new Colour(245, 245, 245);
        leftCamera.Add(new RotatingSquare(CamWidth / 2f, CamHeight / 2f, Colour.Black));
        Add(leftCamera);

        var rightCamera = Camera.WithView(405, CamTop, CamWidth, CamHeight, 0, 0, CamWidth, CamHeight, samples: MsaaSamples.X4);
        rightCamera.BackgroundColour = new Colour(245, 245, 245);
        rightCamera.Add(new RotatingSquare(CamWidth / 2f, CamHeight / 2f, Colour.Black));
        Add(rightCamera);
    }
}
