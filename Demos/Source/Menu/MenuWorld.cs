using System;
using System.Collections.Generic;
using BabyBearsEngine.Demos.Source;

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

    public MenuWorld(IEnumerable<DemoWorld> demoWorlds)
    {
        foreach (var world in demoWorlds)
        {
            AddDemoButton(world);
        }
    }

    private void AddDemoButton(DemoWorld world)
    {
        var (x, y) = NextButtonPosition();
        Add(new DemoButton(x, y, world));
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
