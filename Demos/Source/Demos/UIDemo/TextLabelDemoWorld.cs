using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class TextLabelDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int Row1Y = 160;
    private const int Row2Y = 300;

    public override string Name => "TextLabel & Tooltip";

    public TextLabelDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(LabelLeft, Row1Y, 180, 50, "TextLabel:"));
        TextLabel textLabel = new(WidgetLeft, Row1Y, 200, 50,
            new TextTheme(new FontDefinition("Times New Roman", 18), Colour.Black),
            "Hello from TextLabel!",
            backgroundColour: new Colour(240, 240, 240),
            borderColour: Colour.Black);
        Add(textLabel);
        Button changeText = new(WidgetLeft + 210, Row1Y + 10, 120, 30,
            ButtonTheme.FromColour(new Colour(160, 200, 255)), "Change text");
        changeText.LeftClicked += (_, _) =>
            textLabel.Text = textLabel.Text == "Hello from TextLabel!" ? "Text changed!" : "Hello from TextLabel!";
        Add(changeText);

        Add(MakeLabel(LabelLeft, Row2Y, 180, 50, "Tooltip:"));
        Button tooltipTarget = new(WidgetLeft, Row2Y, 180, 50,
            ButtonTheme.FromColour(new Colour(255, 200, 160)), "Hover for tooltip");
        Add(tooltipTarget);

        SimpleToolTip tooltip = new(WidgetLeft, Row2Y + 60, 180, 30,
            TooltipTheme.Default, "Hello from the tooltip!");
        Overlay.Add(tooltip);

        tooltipTarget.MouseHovered += (_, _) => tooltip.Show();
        tooltipTarget.MouseHoverStopped += (_, _) => tooltip.Hide();
        tooltipTarget.MouseExited += (_, _) => tooltip.Hide();
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
