using System;
using BabyBearsEngine.Demos.Source.Menu;

namespace BabyBearsEngine.Demos.Source;

internal abstract class DemoWorld : World
{
    private readonly Func<World> _menuWorldFactory;

    protected DemoWorld(Func<World> menuWorldFactory)
    {
        _menuWorldFactory = menuWorldFactory;
        AddCommonControls();
    }

    public abstract string Name { get; }

    protected void AddCommonControls()
    {
        Add(new ReturnToMainMenuButton(_menuWorldFactory));
    }
}
