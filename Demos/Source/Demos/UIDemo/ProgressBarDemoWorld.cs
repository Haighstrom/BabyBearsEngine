using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class ProgressBarDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int Row1Y = 160;
    private const int Row2Y = 290;
    private const double FillRate = 0.4;

    private readonly ProgressBar _progressBar;
    private bool _filling = false;

    public override string Name => "Progress Bars";

    public ProgressBarDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(LabelLeft, Row1Y, 180, 50, "Progress bar:"));
        _progressBar = new ProgressBar(WidgetLeft, Row1Y + 10, 260, 30, ProgressBarTheme.Default);
        Add(_progressBar);

        Button fillButton = new(WidgetLeft + 270, Row1Y + 10, 120, 30,
            ButtonTheme.FromColour(new Colour(160, 200, 255)), "Hold to fill");
        fillButton.LeftPressed += (_, _) => _filling = true;
        fillButton.LeftClicked += (_, _) => _filling = false;
        fillButton.MouseExited += (_, _) => _filling = false;
        Add(fillButton);

        Add(MakeLabel(LabelLeft, Row2Y, 180, 50, "Timed bar (3 s):"));
        TimedProgressBar timedBar = new(WidgetLeft, Row2Y + 10, 260, 30, ProgressBarTheme.Default, 3.0);
        timedBar.BarFilled += (_, _) => timedBar.Restart();
        Add(timedBar);
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
