using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class ProgressBarDemoWorld : DemoWorld
{
    private const int LabelLeft = 50;
    private const int WidgetLeft = 250;
    private const int Row1Y = 160;
    private const int Row2Y = 290;
    private const int Row3Y = 410;
    private const int Row4Y = 500;
    private const double FillRate = 0.4;

    private readonly ProgressBar _progressBar;
    private readonly RadialProgressBar _radialPieBar;
    private bool _filling = false;
    private bool _radialFilling = false;

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

        Add(MakeLabel(LabelLeft, Row3Y, 180, 80, "Radial pie:"));
        _radialPieBar = new RadialProgressBar(WidgetLeft, Row3Y, 80, 80, RadialProgressBarTheme.Default);
        Add(_radialPieBar);

        Button radialFillButton = new(WidgetLeft + 100, Row3Y + 25, 120, 30,
            ButtonTheme.FromColour(new Colour(160, 200, 255)), "Hold to fill");
        radialFillButton.LeftPressed += (_, _) => _radialFilling = true;
        radialFillButton.LeftClicked += (_, _) => _radialFilling = false;
        radialFillButton.MouseExited += (_, _) => _radialFilling = false;
        Add(radialFillButton);

        Add(MakeLabel(LabelLeft, Row4Y, 180, 80, "Radial ring (timed 3 s):"));
        RadialProgressBarTheme ringTheme = RadialProgressBarTheme.FromColours(
            new Colour(60, 60, 60), new Colour(80, 200, 80), RadialFillStyle.Ring);
        TimedRadialProgressBar timedRadialBar = new(WidgetLeft, Row4Y, 80, 80, ringTheme, 3.0);
        timedRadialBar.Filled += (_, _) => timedRadialBar.Restart();
        Add(timedRadialBar);
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

        if (_radialFilling)
        {
            _radialPieBar.AmountFilled += (float)(elapsed * FillRate);

            if (_radialPieBar.AmountFilled >= 1.0f)
            {
                _radialPieBar.AmountFilled = 0.0f;
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
