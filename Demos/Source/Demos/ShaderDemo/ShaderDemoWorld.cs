using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.ShaderDemo;

internal class ShaderDemoWorld(Func<World> menuWorldFactory) : DemoWorld(menuWorldFactory)
{
    public override string Name => "Shader";
}
