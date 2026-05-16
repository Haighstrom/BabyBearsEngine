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

    public MenuWorld(IEnumerable<MenuEntry> entries, Func<World>? backFactory = null)
    {
        foreach (MenuEntry entry in entries)
        {
            AddEntryButton(entry);
        }

        if (backFactory is not null)
        {
            Add(new BackButton(Window.Width - 85, 5, backFactory));
        }
    }

    private void AddEntryButton(MenuEntry entry)
    {
        var (x, y) = NextButtonPosition();

        if (entry.Style == MenuEntryStyle.Submenu)
        {
            Add(new SubmenuButton(x, y, entry.Name, entry.Factory));
        }
        else
        {
            Add(new DemoButton(x, y, entry.Name, entry.Factory));
        }

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
