using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.AnimationDemo;

internal class AnimationDemoWorld(Func<World> menuWorldFactory) : DemoWorld(menuWorldFactory)
{
    public override string Name => "Animation";
}
