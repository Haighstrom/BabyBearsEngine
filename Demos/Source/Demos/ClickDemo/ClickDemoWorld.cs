using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.ClickDemo;

internal class ClickDemoWorld : DemoWorld
{
    public override string Name => "Click Demo";

    public ClickDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(new Button(75, 75, 150, 150, Colour.Orange));
        Add(new Button(50, 50, 100, 100, Colour.Green));
        Add(new Button(100, 100, 100, 100, Colour.Fuchsia));

        Add(new DraggablePanel(300, 50, 100, 100, Colour.Orange));
    }
}
