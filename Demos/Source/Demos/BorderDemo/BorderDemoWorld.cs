using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.BorderDemo;

internal class BorderDemoWorld : DemoWorld
{
    private const float Col1X = 100f;
    private const float Col2X = 320f;
    private const float Col3X = 540f;
    private const float MaxThickness = 30f;
    private const float MinThickness = 1f;
    private const float RectH = 100f;
    private const float RectW = 160f;
    private const float RectY = 155f;

    private static readonly Colour[] s_borderPalette =
    [
        Colour.CornflowerBlue, Colour.Crimson, Colour.ForestGreen,
        Colour.Gold, Colour.BlueViolet, Colour.DimGray,
    ];

    private static readonly Colour[] s_fillPalette =
    [
        Colour.White, Colour.LightGray, Colour.MediumTurquoise,
        Colour.YellowGreen, Colour.HotPink, Colour.Transparent,
    ];

    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private int _borderColourIndex = 0;
    private int _fillColourIndex = 0;
    private readonly ColourGraphic[] _fills = new ColourGraphic[3];
    private readonly BorderedRectangleGraphic[] _rects = new BorderedRectangleGraphic[3];
    private float _thickness = 10f;
    private readonly TextGraphic _thicknessLabel;

    public BorderDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        Add(new TextGraphic(s_titleFont, "Bordered Rectangle Demo", Colour.DimGray, 0f, 50f, 800f, 24f)
        {
            HAlignment = HAlignment.Centred,
        });

        // Column headers
        AddColumnLabel(Col1X, 100f, "Inside");
        AddColumnLabel(Col2X, 100f, "Outside");
        AddColumnLabel(Col3X, 100f, "Centred");

        // Explanatory notes
        AddNote(Col1X, 120f, "border draws inside bounds");
        AddNote(Col2X, 120f, "border extends beyond bounds");
        AddNote(Col3X, 120f, "border straddles the edge");

        // Fill backgrounds (added first so they render behind the border frames)
        float[] colXs = [Col1X, Col2X, Col3X];
        for (int i = 0; i < 3; i++)
        {
            _fills[i] = new ColourGraphic(s_fillPalette[0], colXs[i], RectY, RectW, RectH);
            Add(_fills[i]);
        }

        // Border frames — no fill, independent of the ColourGraphic backgrounds
        _rects[0] = new BorderedRectangleGraphic(Col1X, RectY, RectW, RectH, _thickness, s_borderPalette[0], BorderPosition.Inside);
        _rects[1] = new BorderedRectangleGraphic(Col2X, RectY, RectW, RectH, _thickness, s_borderPalette[0], BorderPosition.Outside);
        _rects[2] = new BorderedRectangleGraphic(Col3X, RectY, RectW, RectH, _thickness, s_borderPalette[0], BorderPosition.Centred);
        Add(_rects[0]);
        Add(_rects[1]);
        Add(_rects[2]);

        // Thickness controls
        float ctrlY = 315f;
        Add(new TextGraphic(s_font, "Thickness:", Colour.DimGray, 195f, ctrlY, 120f, 28f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        Button minus = new(325f, ctrlY, 36f, 28f, ButtonTheme.FromColour(new Colour(180, 90, 90)), "-");
        minus.LeftClicked += (_, _) => AdjustThickness(-1f);
        Add(minus);

        _thicknessLabel = new TextGraphic(s_font, FormatThickness(), Colour.DimGray, 365f, ctrlY, 70f, 28f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_thicknessLabel);

        Button plus = new(439f, ctrlY, 36f, 28f, ButtonTheme.FromColour(new Colour(70, 150, 70)), "+");
        plus.LeftClicked += (_, _) => AdjustThickness(+1f);
        Add(plus);

        // Colour cycle buttons
        float colourY = 365f;
        Button cycleBorder = new(215f, colourY, 170f, 30f, ButtonTheme.FromColour(new Colour(80, 120, 200)), "Cycle Border Colour");
        cycleBorder.LeftClicked += (_, _) =>
        {
            _borderColourIndex = (_borderColourIndex + 1) % s_borderPalette.Length;
            Colour c = s_borderPalette[_borderColourIndex];
            foreach (BorderedRectangleGraphic r in _rects)
            {
                r.BorderColour = c;
            }
        };
        Add(cycleBorder);

        Button cycleFill = new(415f, colourY, 170f, 30f, ButtonTheme.FromColour(new Colour(80, 120, 200)), "Cycle Fill Colour");
        cycleFill.LeftClicked += (_, _) =>
        {
            _fillColourIndex = (_fillColourIndex + 1) % s_fillPalette.Length;
            Colour c = s_fillPalette[_fillColourIndex];
            foreach (ColourGraphic f in _fills)
            {
                f.Colour = c;
            }
        };
        Add(cycleFill);
    }

    public override string Name => "Border";

    private void AddColumnLabel(float x, float y, string text)
    {
        Add(new TextGraphic(s_font, text, Colour.DimGray, x, y, RectW, 20f)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    private void AddNote(float x, float y, string text)
    {
        Add(new TextGraphic(s_font, text, new Colour(110, 110, 110), x, y, RectW, 16f)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    private void AdjustThickness(float delta)
    {
        _thickness = Math.Clamp(_thickness + delta, MinThickness, MaxThickness);
        foreach (BorderedRectangleGraphic r in _rects)
        {
            r.BorderThickness = _thickness;
        }

        _thicknessLabel.Text = FormatThickness();
    }

    private string FormatThickness() => $"{_thickness:0} px";
}
