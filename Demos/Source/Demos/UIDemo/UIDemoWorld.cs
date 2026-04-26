using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class UIDemoWorld : DemoWorld
{
    public override string Name => "UI Demo";

    public UIDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
