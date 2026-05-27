using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class ScrollbarDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int Row1Y = 160;
    private const int Row2Y = 300;

    public override string Name => "Scrollbars";

    public ScrollbarDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(LabelLeft, Row1Y, 180, 50, "Horizontal:"));
        Scrollbar hScrollbar = new(WidgetLeft, Row1Y + 15, 300, 20,
            ScrollbarDirection.Horizontal, ScrollbarTheme.Default);
        TextGraphic hScrollLabel = MakeLabel(WidgetLeft + 320, Row1Y, 100, 50, FormatAmount(hScrollbar.AmountFilled));
        hScrollbar.ScrollChanged += (_, e) => hScrollLabel.Text = FormatAmount(e.NewValue);
        Add(hScrollbar);
        Add(hScrollLabel);

        Add(MakeLabel(LabelLeft, Row2Y, 180, 50, "Vertical:"));
        Scrollbar vScrollbar = new(WidgetLeft, Row2Y, 20, 150,
            ScrollbarDirection.Vertical, ScrollbarTheme.Default);
        TextGraphic vScrollLabel = MakeLabel(WidgetLeft + 40, Row2Y, 100, 50, FormatAmount(vScrollbar.AmountFilled));
        vScrollbar.ScrollChanged += (_, e) => vScrollLabel.Text = FormatAmount(e.NewValue);
        Add(vScrollbar);
        Add(vScrollLabel);
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
