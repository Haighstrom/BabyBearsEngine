using System;
using System.Collections.Generic;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class MenuWorld : World
{
    private const int BottomMargin = 20;
    private const int ButtonHeight = 60;
    private const int ButtonWidth = 120;
    private const int HorizontalGap = 10;
    private const int StartX = 20;
    private const int StartY = 20;
    private const int VerticalGap = 5;

    private int _buttonCount = 0;

    public MenuWorld(IEnumerable<(string Name, Func<World> Factory)> demos)
    {
        foreach (var (name, factory) in demos)
        {
            AddDemoButton(name, factory);
        }
    }

    private void AddDemoButton(string name, Func<World> factory)
    {
        var (x, y) = NextButtonPosition();
        Add(new DemoButton(x, y, name, factory));
        _buttonCount++;
    }

    private (int x, int y) NextButtonPosition()
    {
        int step = ButtonHeight + VerticalGap;
        int buttonsPerColumn = Math.Max(1, (Window.Height - StartY - BottomMargin) / step);

        int column = _buttonCount / buttonsPerColumn;
        int row = _buttonCount % buttonsPerColumn;

        return (StartX + column * (ButtonWidth + HorizontalGap), StartY + row * step);
    }
}
