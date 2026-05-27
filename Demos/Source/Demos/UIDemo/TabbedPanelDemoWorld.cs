using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class TabbedPanelDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int RowY = 230;

    public override string Name => "Tabbed Panel";

    public TabbedPanelDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(LabelLeft, RowY, 180, 50, "Tabbed panel:"));
        TabbedPanel tabbedPanel = new(WidgetLeft, RowY + 5, 300, 80, 25, TabbedPanelTheme.Default, tabSpacing: 4);

        Tab statsTab = new(80, 25, "Stats", TabbedPanelTheme.Default);
        statsTab.AddContent(MakeLabel(5, 5, 280, 55, "Character stats go here"));
        tabbedPanel.AddTab(statsTab);

        Tab tasksTab = new(80, 25, "Tasks", TabbedPanelTheme.Default);
        tasksTab.AddContent(MakeLabel(5, 5, 280, 55, "Task list goes here"));
        tabbedPanel.AddTab(tasksTab);

        Add(tabbedPanel);
    }

    private static TextGraphic MakeLabel(int x, int y, int width, int height, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }
}
