using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.MouseDemo;

internal class MouseDemoWorld : DemoWorld
{
    public override string Name => "Mouse Demo";

    public MouseDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
