using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;

internal class ClickTheBearDemoWorld : DemoWorld
{
    public override string Name => "Click The Bear";

    public ClickTheBearDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
