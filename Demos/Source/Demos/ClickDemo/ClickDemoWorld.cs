using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.ClickDemo;

internal class ClickDemoWorld : DemoWorld
{
    private const int Col1X = 20;
    private const int Col2X = 410;
    private const int Row1Y = 65;
    private const int Row2Y = 400;
    private const int SectionW = 350;

    public override string Name => "Click Demo";

    public ClickDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        AddClickEventsSection();
        AddTopMostOnlySection();
        AddClickThroughSection();
    }

    private static TextGraphic MakeHeader(int x, int y, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 16), text, Colour.Black, x, y, SectionW, 25)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }

    private static TextGraphic MakeLabel(int x, int y, int w, int h, string text, int fontSize = 13)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", fontSize), text, Colour.Black, x, y, w, h)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }

    private void AddClickEventsSection()
    {
        int x = Col1X, y = Row1Y;

        Add(MakeHeader(x, y, "Click Events"));

        Button button = new(x + 70, y + 35, 210, 110, ButtonTheme.FromColour(new Colour(100, 160, 255)), "Click me!");
        TextGraphic status = MakeLabel(x, y + 160, SectionW, 35, "- hover, click, double-click -", 16);

        button.MouseEntered += (_, _) => status.Text = "MouseEntered";
        button.MouseExited += (_, _) => status.Text = "MouseExited";
        button.LeftPressed += (_, _) => status.Text = "LeftPressed";
        button.LeftClicked += (_, _) => status.Text = "LeftClicked";
        button.LeftDoubleClicked += (_, _) => status.Text = "LeftDoubleClicked";
        button.MouseHovered += (_, _) => status.Text = "MouseHovered  (0.5 s dwell)";
        button.MouseHoverStopped += (_, _) => status.Text = "MouseHoverStopped";

        Add(button);
        Add(status);
    }

    private void AddTopMostOnlySection()
    {
        int x = Col2X, y = Row1Y;
        int orangeClicks = 0, blueClicks = 0;

        Add(MakeHeader(x, y, "Top-Most Only  (default)"));
        Add(MakeLabel(x, y + 28, SectionW, 20, "Click in the overlapping area:"));

        Button orange = new(x, y + 55, 190, 85, ButtonTheme.FromColour(new Colour(255, 150, 50)), "Orange");
        Button blue = new(x + 130, y + 100, 190, 85, ButtonTheme.FromColour(new Colour(80, 120, 255)), "Blue");

        TextGraphic orangeCount = MakeLabel(x, y + 200, SectionW, 28, "Orange: 0 clicks", 15);
        TextGraphic blueCount = MakeLabel(x + 130, y + 200, SectionW - 130, 28, "Blue: 0 clicks", 15);

        orange.LeftClicked += (_, _) => orangeCount.Text = $"Orange: {++orangeClicks} clicks";
        blue.LeftClicked += (_, _) => blueCount.Text = $"Blue: {++blueClicks} clicks";

        Add(MakeLabel(x, y + 236, SectionW, 22, "In the overlap, only Blue responds."));
        Add(orangeCount);
        Add(blueCount);
        Add(orange);
        Add(blue);
    }

    private void AddClickThroughSection()
    {
        int x = Col2X, y = Row2Y;
        int orangeClicks = 0, blueClicks = 0;

        Add(MakeHeader(x, y, "Click-Through  (ClickThrough = true)"));
        Add(MakeLabel(x, y + 28, SectionW, 20, "Click in the overlapping area:"));

        Button orange = new(x, y + 55, 190, 85, ButtonTheme.FromColour(new Colour(255, 150, 50)), "Orange");
        Button blue = new(x + 130, y + 100, 190, 85, ButtonTheme.FromColour(new Colour(80, 120, 255)), "Blue")
        {
            ClickThrough = true,
        };

        TextGraphic orangeCount = MakeLabel(x, y + 200, SectionW, 28, "Orange: 0 clicks", 15);
        TextGraphic blueCount = MakeLabel(x + 130, y + 200, SectionW - 130, 28, "Blue: 0 clicks", 15);

        orange.LeftClicked += (_, _) => orangeCount.Text = $"Orange: {++orangeClicks} clicks";
        blue.LeftClicked += (_, _) => blueCount.Text = $"Blue: {++blueClicks} clicks";

        Add(MakeLabel(x, y + 236, SectionW, 22, "Both counters increment when clicking the overlap."));
        Add(orangeCount);
        Add(blueCount);
        Add(orange);
        Add(blue);
    }
}
