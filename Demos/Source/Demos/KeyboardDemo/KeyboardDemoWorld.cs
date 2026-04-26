using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.KeyboardDemo;

internal class KeyboardDemoWorld : DemoWorld
{
    public override string Name => "Keyboard Demo";

    public KeyboardDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
