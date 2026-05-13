using System;
using System.Diagnostics;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Rendering.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class UIDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int RowHeight = 80;
    private const int FirstRowY = 100;

    private readonly ProgressBar _progressBar;

    public override string Name => "UI Demo";

    public UIDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(LabelLeft, FirstRowY, 180, 50, "Checkbox:"));
        Add(new Checkbox(WidgetLeft, FirstRowY, 50, 50, CheckboxTheme.Default));
        Add(new Checkbox(WidgetLeft + 70, FirstRowY, 50, 50, CheckboxTheme.Default, isChecked: true));

        Add(MakeLabel(LabelLeft, FirstRowY + RowHeight, 180, 50, "Cycling button:"));
        Add(new CyclingValueButton<string>(
            WidgetLeft, FirstRowY + RowHeight, 180, 50,
            ButtonTheme.FromColour(new Colour(160, 200, 255)),
            ["Easy", "Normal", "Hard", "Nightmare"]));

        Add(MakeLabel(LabelLeft, FirstRowY + 2 * RowHeight, 180, 50, "Progress bar:"));
        _progressBar = new ProgressBar(WidgetLeft, FirstRowY + 2 * RowHeight, 300, 30, ProgressBarTheme.Default);
        Add(_progressBar);

        Add(MakeLabel(LabelLeft, FirstRowY + 3 * RowHeight, 180, 50, "Tooltip:"));
        Button tooltipTarget = new(WidgetLeft, FirstRowY + 3 * RowHeight, 180, 50,
            ButtonTheme.FromColour(new Colour(255, 200, 160)), "Hover for tooltip");
        Add(tooltipTarget);

        SimpleToolTip tooltip = new(WidgetLeft, FirstRowY + 3 * RowHeight + 60, 180, 30,
            TooltipTheme.Default, "Hello from the tooltip!");
        Add(tooltip);

        tooltipTarget.MouseHovered += (_, _) => tooltip.Show();
        tooltipTarget.MouseHoverStopped += (_, _) => tooltip.Hide();
        tooltipTarget.MouseExited += (_, _) => tooltip.Hide();
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);
        _progressBar.AmountFilled = (float)((Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency % 5d) / 5d);
    }

    private static TextImage MakeLabel(int x, int y, int width, int height, string text)
    {
        return new TextImage(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }
}
