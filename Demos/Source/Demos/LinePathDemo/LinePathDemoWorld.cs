using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.LinePathDemo;

internal class LinePathDemoWorld : DemoWorld
{
    private const float MaxThickness = 20f;
    private const float MinThickness = 1f;
    private const int WaveSamples = 25;

    private static readonly Colour[] s_palette = [Colour.OrangeRed, Colour.MediumTurquoise, Colour.BlueViolet];

    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private bool _dashed = false;
    private readonly LinePathGraphic[] _paths = new LinePathGraphic[3];
    private int _paletteOffset = 0;
    private float _thickness = 6f;
    private readonly TextGraphic _thicknessLabel;

    public LinePathDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        Add(new TextGraphic(s_titleFont, "Line Path Demo", Colour.DimGray, 0f, 50f, 800f, 24f)
        {
            HAlignment = HAlignment.Centred,
        });
        Add(new TextGraphic(s_font, "A list of points, joined and mitered at each shared vertex", Colour.DimGray, 0f, 85f, 800f, 18f)
        {
            HAlignment = HAlignment.Centred,
        });

        AddCaption(20f, 130f, "Zigzag (open path)");
        AddCaption(300f, 130f, "Hexagon (first point == last -> closed loop)");
        AddCaption(560f, 130f, "Sine wave (25 points)");

        _paths[0] = new LinePathGraphic(ZigzagPoints(), s_palette[0], _thickness);
        _paths[1] = new LinePathGraphic(HexagonPoints(), s_palette[1], _thickness);
        _paths[2] = new LinePathGraphic(WavePoints(), s_palette[2], _thickness);
        foreach (LinePathGraphic path in _paths)
        {
            Add(path);
        }

        // Controls
        float controlsY = 520f;
        Add(new TextGraphic(s_font, "Thickness:", Colour.DimGray, 260f, controlsY, 120f, 28f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        Button minus = new(390f, controlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(180, 90, 90)), "-");
        minus.LeftClicked += (_, _) => AdjustThickness(-1f);
        Add(minus);

        _thicknessLabel = new TextGraphic(s_font, FormatThickness(), Colour.DimGray, 430f, controlsY, 60f, 28f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_thicknessLabel);

        Button plus = new(494f, controlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(70, 150, 70)), "+");
        plus.LeftClicked += (_, _) => AdjustThickness(+1f);
        Add(plus);

        Button cycleColour = new(560f, controlsY, 130f, 28f, ButtonTheme.FromColour(new Colour(80, 120, 200)), "Cycle Colour");
        cycleColour.LeftClicked += (_, _) => CyclePalette();
        Add(cycleColour);

        Button toggleDashed = new(700f, controlsY, 90f, 28f, ButtonTheme.FromColour(new Colour(150, 100, 180)), "Dashed");
        toggleDashed.LeftClicked += (_, _) => ToggleDashed();
        Add(toggleDashed);
    }

    public override string Name => "Line Path";

    private static Point[] HexagonPoints()
    {
        const int SideCount = 6;
        Point centre = new(400f, 330f);
        const float Radius = 130f;

        Point[] points = new Point[SideCount + 1];
        for (int sideIndex = 0; sideIndex < SideCount; sideIndex++)
        {
            float angleRadians = (sideIndex * (360f / SideCount) - 90f) * MathF.PI / 180f;
            points[sideIndex] = new Point(centre.X + Radius * MathF.Cos(angleRadians), centre.Y + Radius * MathF.Sin(angleRadians));
        }
        points[SideCount] = points[0]; // repeat the first point to close the loop

        return points;
    }

    private static Point[] WavePoints()
    {
        const float StartX = 560f;
        const float EndX = 780f;
        const float CentreY = 330f;
        const float Amplitude = 110f;
        const float Cycles = 2f;

        Point[] points = new Point[WaveSamples];
        for (int sampleIndex = 0; sampleIndex < WaveSamples; sampleIndex++)
        {
            float t = sampleIndex / (float)(WaveSamples - 1);
            float x = StartX + t * (EndX - StartX);
            float y = CentreY + Amplitude * MathF.Sin(t * Cycles * 2f * MathF.PI);
            points[sampleIndex] = new Point(x, y);
        }

        return points;
    }

    private static Point[] ZigzagPoints() =>
    [
        new(60f, 220f), new(150f, 190f), new(95f, 360f),
        new(190f, 280f), new(130f, 460f), new(220f, 400f),
    ];

    private void AddCaption(float x, float y, string text)
    {
        Add(new TextGraphic(s_font, text, new Colour(110, 110, 110), x, y, 240f, 16f)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    private void AdjustThickness(float delta)
    {
        _thickness = Math.Clamp(_thickness + delta, MinThickness, MaxThickness);
        foreach (LinePathGraphic path in _paths)
        {
            path.Thickness = _thickness;
        }

        _thicknessLabel.Text = FormatThickness();
    }

    private void CyclePalette()
    {
        _paletteOffset = (_paletteOffset + 1) % s_palette.Length;
        for (int pathIndex = 0; pathIndex < _paths.Length; pathIndex++)
        {
            _paths[pathIndex].Colour = s_palette[(pathIndex + _paletteOffset) % s_palette.Length];
        }
    }

    private string FormatThickness() => $"{_thickness:0} px";

    private void ToggleDashed()
    {
        _dashed = !_dashed;
        foreach (LinePathGraphic path in _paths)
        {
            path.DashLength = 16f;
            path.GapLength = _dashed ? 10f : 0f;
        }
    }
}
