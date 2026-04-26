using System;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.ClickDemo;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class MenuWorld : World
{
    private const int StartX = 20;
    private const int StartY = 20;
    private const int ButtonWidth = 120;
    private const int ButtonHeight = 60;
    private const int HorizontalGap = 10;
    private const int VerticalGap = 5;
    private const int BottomMargin = 20;

    private int _buttonCount = 0;

    public MenuWorld()
    {
        AddDemoButton((x, y) => new BearSpinnerButton(x, y));
        AddDemoButton((x, y) => new TextDemoButton(x, y));
        AddDemoButton((x, y) => new ClickDemoButton(x, y));
    }

    private void AddDemoButton(Func<int, int, Button> factory)
    {
        var (x, y) = NextButtonPosition();
        Add(factory(x, y));
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
