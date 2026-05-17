using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class UIDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int RowHeight = 80;
    private const int FirstRowY = 100;
    private const double FillRate = 0.4;

    private readonly ProgressBar _progressBar;
    private bool _filling = false;

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
        _progressBar = new ProgressBar(WidgetLeft, FirstRowY + 2 * RowHeight + 10, 260, 30, ProgressBarTheme.Default);
        Add(_progressBar);

        Button fillButton = new(WidgetLeft + 270, FirstRowY + 2 * RowHeight + 10, 120, 30,
            ButtonTheme.FromColour(new Colour(160, 200, 255)), "Hold to fill");
        fillButton.LeftPressed += (_, _) => _filling = true;
        fillButton.LeftClicked += (_, _) => _filling = false;
        fillButton.MouseExited += (_, _) => _filling = false;
        Add(fillButton);

        Add(MakeLabel(LabelLeft, FirstRowY + 3 * RowHeight, 180, 50, "TextLabel:"));
        TextLabel textLabel = new(WidgetLeft, FirstRowY + 3 * RowHeight, 200, 50,
            new TextTheme(new FontDefinition("Times New Roman", 18), Colour.Black),
            "Hello from TextLabel!",
            backgroundColour: new Colour(240, 240, 240),
            borderColour: Colour.Black);
        Add(textLabel);
        Button changeText = new(WidgetLeft + 210, FirstRowY + 3 * RowHeight + 10, 120, 30,
            ButtonTheme.FromColour(new Colour(160, 200, 255)), "Change text");
        changeText.LeftClicked += (_, _) =>
            textLabel.Text = textLabel.Text == "Hello from TextLabel!" ? "Text changed!" : "Hello from TextLabel!";
        Add(changeText);

        Add(MakeLabel(LabelLeft, FirstRowY + 4 * RowHeight, 180, 50, "Tooltip:"));
        Button tooltipTarget = new(WidgetLeft, FirstRowY + 4 * RowHeight, 180, 50,
            ButtonTheme.FromColour(new Colour(255, 200, 160)), "Hover for tooltip");
        Add(tooltipTarget);

        SimpleToolTip tooltip = new(WidgetLeft, FirstRowY + 4 * RowHeight + 60, 180, 30,
            TooltipTheme.Default, "Hello from the tooltip!");
        Overlay.Add(tooltip);

        tooltipTarget.MouseHovered += (_, _) => tooltip.Show();
        tooltipTarget.MouseHoverStopped += (_, _) => tooltip.Hide();
        tooltipTarget.MouseExited += (_, _) => tooltip.Hide();
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        if (_filling)
        {
            _progressBar.AmountFilled += (float)(elapsed * FillRate);

            if (_progressBar.AmountFilled >= 1.0f)
            {
                _progressBar.AmountFilled = 0.0f;
            }
        }
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
