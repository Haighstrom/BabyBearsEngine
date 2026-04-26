using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class TextDemoWorld : DemoWorld
{
    public override string Name => "Text Demo";

    public TextDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
