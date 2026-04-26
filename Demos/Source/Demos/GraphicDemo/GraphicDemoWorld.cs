using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.GraphicDemo;

internal class GraphicDemoWorld : DemoWorld
{
    public override string Name => "Graphic Demo";

    public GraphicDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
