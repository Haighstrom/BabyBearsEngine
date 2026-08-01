using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.LineDemo;

internal class LineDemoWorld : DemoWorld
{
    private const float FanRadius = 110f;
    private const float MaxThickness = 24f;
    private const float MinThickness = 1f;

    private static readonly Point FanCentre = new(400f, 225f);

    private static readonly Colour[] s_fanPalette =
    [
        Colour.Crimson, Colour.OrangeRed, Colour.Orange, Colour.Gold, Colour.YellowGreen,
        Colour.ForestGreen, Colour.MediumTurquoise, Colour.DodgerBlue, Colour.BlueViolet, Colour.HotPink,
    ];

    private static readonly FontDefinition s_captionFont = new("Times New Roman", 12);
    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private readonly LineGraphic[] _fanLines = new LineGraphic[s_fanPalette.Length];
    private int _paletteOffset = 0;
    private float _thickness = 6f;
    private readonly TextGraphic _thicknessLabel;

    public LineDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        Add(new TextGraphic(s_titleFont, "Line Demo", Colour.DimGray, 0f, 50f, 800f, 24f)
        {
            HAlignment = HAlignment.Centred,
        });

        // Fan of lines — one LineGraphic per palette colour, radiating from a shared centre point.
        Add(new TextGraphic(s_font, "Start / End points, Colour, Thickness", Colour.DimGray, 0f, 85f, 800f, 18f)
        {
            HAlignment = HAlignment.Centred,
        });

        for (int fanIndex = 0; fanIndex < _fanLines.Length; fanIndex++)
        {
            Point end = FanEndPoint(fanIndex);
            LineGraphic line = new(FanCentre, end, s_fanPalette[fanIndex], _thickness);
            _fanLines[fanIndex] = line;
            Add(line);
        }

        // Fan controls
        float fanControlsY = 350f;
        Add(new TextGraphic(s_font, "Thickness:", Colour.DimGray, 260f, fanControlsY, 120f, 28f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        Button minus = new(390f, fanControlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(180, 90, 90)), "-");
        minus.LeftClicked += (_, _) => AdjustThickness(-1f);
        Add(minus);

        _thicknessLabel = new TextGraphic(s_font, FormatThickness(), Colour.DimGray, 430f, fanControlsY, 60f, 28f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_thicknessLabel);

        Button plus = new(494f, fanControlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(70, 150, 70)), "+");
        plus.LeftClicked += (_, _) => AdjustThickness(+1f);
        Add(plus);

        Button cycleColour = new(560f, fanControlsY, 130f, 28f, ButtonTheme.FromColour(new Colour(80, 120, 200)), "Cycle Colour");
        cycleColour.LeftClicked += (_, _) => CyclePalette();
        Add(cycleColour);

        // ThicknessInPixels comparison — same line data rendered inside two cameras at different
        // zoom levels. The pixel-thickness line stays a constant 6px; the world-thickness line
        // scales with the camera's tile size, same as any other world-space geometry would.
        Add(new TextGraphic(s_font, "ThicknessInPixels: constant on-screen width vs. world-space width that scales with zoom", Colour.DimGray, 0f, 390f, 800f, 18f)
        {
            HAlignment = HAlignment.Centred,
        });
        Add(new TextGraphic(s_captionFont, "Blue = ThicknessInPixels: true (6px)      Orange = ThicknessInPixels: false (0.3 world units)", new Colour(110, 110, 110), 0f, 409f, 800f, 16f)
        {
            HAlignment = HAlignment.Centred,
        });

        AddZoomCamera(x: 20f, tileSize: 15f, caption: "Zoom x1 - 15px/tile");
        AddZoomCamera(x: 410f, tileSize: 45f, caption: "Zoom x3 - 45px/tile");
    }

    public override string Name => "Line";

    private static Point FanEndPoint(int fanIndex)
    {
        float angleDegrees = fanIndex * (360f / s_fanPalette.Length) - 90f;
        float angleRadians = angleDegrees * MathF.PI / 180f;
        return new Point(
            FanCentre.X + FanRadius * MathF.Cos(angleRadians),
            FanCentre.Y + FanRadius * MathF.Sin(angleRadians));
    }

    private void AddZoomCamera(float x, float tileSize, string caption)
    {
        const float CameraWidth = 370f;
        const float CameraHeight = 140f;
        const float CaptionY = 428f;
        const float CameraY = 446f;

        Add(new TextGraphic(s_captionFont, caption, Colour.DimGray, x, CaptionY, CameraWidth, 16f)
        {
            HAlignment = HAlignment.Centred,
        });

        Camera camera = Camera.WithTileSize(x, CameraY, CameraWidth, CameraHeight, tileSize, tileSize);
        camera.BackgroundColour = new Colour(238, 238, 238);

        camera.Add(new LineGraphic(new Point(1f, 1f), new Point(6f, 1f), Colour.DodgerBlue, thickness: 6f));
        camera.Add(new LineGraphic(new Point(1f, 2f), new Point(6f, 2f), Colour.Orange, thickness: 0.3f, thicknessInPixels: false));

        Add(camera);
    }

    private void AdjustThickness(float delta)
    {
        _thickness = Math.Clamp(_thickness + delta, MinThickness, MaxThickness);
        foreach (LineGraphic line in _fanLines)
        {
            line.Thickness = _thickness;
        }

        _thicknessLabel.Text = FormatThickness();
    }

    private void CyclePalette()
    {
        _paletteOffset = (_paletteOffset + 1) % s_fanPalette.Length;
        for (int fanIndex = 0; fanIndex < _fanLines.Length; fanIndex++)
        {
            _fanLines[fanIndex].Colour = s_fanPalette[(fanIndex + _paletteOffset) % s_fanPalette.Length];
        }
    }

    private string FormatThickness() => $"{_thickness:0} px";
}
