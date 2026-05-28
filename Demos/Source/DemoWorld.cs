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

    /// <summary>
    /// Where the shared "← Back" button sits. Defaults to the top-left corner; a demo whose own
    /// content fills the top-left can override this to move the button to the top-right instead.
    /// </summary>
    protected virtual bool BackButtonTopRight => false;

    protected void AddCommonControls()
    {
        // 80px wide button + 5px right margin = 85px inset from the right edge, mirroring MenuWorld.
        float x = BackButtonTopRight ? Window.Width - 85 : 5;
        Add(new BackButton(x, 5, _menuWorldFactory));
    }
}
