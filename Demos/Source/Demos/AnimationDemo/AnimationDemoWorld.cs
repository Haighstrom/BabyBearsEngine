using System;

namespace BabyBearsEngine.Demos.Source.Demos.AnimationDemo;

internal class AnimationDemoWorld(Func<World> menuWorldFactory) : DemoWorld(menuWorldFactory)
{
    public override string Name => "Animation";
}
