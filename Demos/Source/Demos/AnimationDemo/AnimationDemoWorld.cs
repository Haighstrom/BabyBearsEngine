using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.AnimationDemo;

internal class AnimationDemoWorld : DemoWorld
{
    public override string Name => "Animation Demo";

    public AnimationDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
