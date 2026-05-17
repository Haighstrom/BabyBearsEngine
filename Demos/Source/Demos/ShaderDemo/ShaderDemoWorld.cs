using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.ShaderDemo;

internal class ShaderDemoWorld : DemoWorld
{
    public override string Name => "Shader";

    public ShaderDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
    }
}
