using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class UIDemoWorld2 : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int RowHeight = 80;
    private const int FirstRowY = 60;

    public override string Name => "UI Demo 2";

    public UIDemoWorld2(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        // Scrollbar (horizontal)
        Add(MakeLabel(LabelLeft, FirstRowY, 180, 50, "Scrollbar (H):"));
        Scrollbar hScrollbar = new(WidgetLeft, FirstRowY + 15, 300, 20, ScrollbarDirection.Horizontal, ScrollbarTheme.Default);
        TextGraphic hScrollLabel = MakeLabel(WidgetLeft + 320, FirstRowY, 100, 50, FormatAmount(hScrollbar.AmountFilled));
        hScrollbar.ScrollChanged += (_, e) => hScrollLabel.Text = FormatAmount(e.NewValue);
        Add(hScrollbar);
        Add(hScrollLabel);

        // Scrollbar (vertical) — 150 px tall, drives the next-row offset
        Add(MakeLabel(LabelLeft, FirstRowY + RowHeight, 180, 50, "Scrollbar (V):"));
        Scrollbar vScrollbar = new(WidgetLeft, FirstRowY + RowHeight, 20, 150, ScrollbarDirection.Vertical, ScrollbarTheme.Default);
        TextGraphic vScrollLabel = MakeLabel(WidgetLeft + 40, FirstRowY + RowHeight, 100, 50, FormatAmount(vScrollbar.AmountFilled));
        vScrollbar.ScrollChanged += (_, e) => vScrollLabel.Text = FormatAmount(e.NewValue);
        Add(vScrollbar);
        Add(vScrollLabel);

        // Content below the V scrollbar starts after it ends (FirstRowY + RowHeight + 150 + gap)
        int belowScrollbars = FirstRowY + RowHeight + 170;

        // Timed progress bar
        Add(MakeLabel(LabelLeft, belowScrollbars, 180, 50, "Timed bar (3 s):"));
        TimedProgressBar timedBar = new(WidgetLeft, belowScrollbars + 15, 300, 30, ProgressBarTheme.Default, 3.0);
        timedBar.BarFilled += (_, _) => timedBar.Restart();
        Add(timedBar);

        // Tabbed panel
        int tabbedY = belowScrollbars + RowHeight;
        Add(MakeLabel(LabelLeft, tabbedY, 180, 50, "Tabbed panel:"));
        TabbedPanel tabbedPanel = new(WidgetLeft, tabbedY + 5, 300, 80, 25, TabbedPanelTheme.Default, tabSpacing: 4);

        Tab statsTab = new(80, 25, "Stats", TabbedPanelTheme.Default);
        statsTab.AddContent(MakeLabel(5, 5, 280, 55, "Character stats go here"));
        tabbedPanel.AddTab(statsTab);

        Tab tasksTab = new(80, 25, "Tasks", TabbedPanelTheme.Default);
        tasksTab.AddContent(MakeLabel(5, 5, 280, 55, "Task list goes here"));
        tabbedPanel.AddTab(tasksTab);

        Add(tabbedPanel);
    }

    private static string FormatAmount(float amount) => $"{amount * 100:0}%";

    private static TextGraphic MakeLabel(int x, int y, int width, int height, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }
}
