using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.ShaderDemo;

internal class ShaderDemoWorld : DemoWorld
{
    public override string Name => "Shader Demo";

    public ShaderDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
