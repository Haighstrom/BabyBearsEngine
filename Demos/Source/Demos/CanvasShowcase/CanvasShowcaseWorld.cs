using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.CanvasShowcase;

/// <summary>
/// A small freehand-drawing app: click-drag on the canvas to paint a stroke with
/// <see cref="LinePathGraphic"/>, appending a point per frame via <see cref="LinePathGraphic.AppendPoint"/>.
/// </summary>
internal class CanvasShowcaseWorld : DemoWorld
{
    private const float CanvasTop = 46f;
    private const float MaxPenSize = 30f;
    private const float MinPenSize = 2f;
    private const float MinPointSpacing = 4f;
    private const float PenSizeStep = 2f;

    private static readonly FontDefinition s_font = new("Times New Roman", 13);

    private static readonly Colour[] s_palette =
    [
        Colour.White, Colour.Black, Colour.Crimson, Colour.Orange,
        Colour.Gold, Colour.ForestGreen, Colour.DodgerBlue, Colour.BlueViolet,
    ];

    private Colour _currentColour = Colour.Black;
    private LinePathGraphic? _currentStroke = null;
    private Point _lastAppendedPoint = Point.Zero;
    private float _penSize = 6f;
    private readonly TextGraphic _penSizeLabel;
    private readonly BorderedRectangleGraphic _selectionHighlight;
    private Point? _strokeStart = null;
    private readonly List<LinePathGraphic> _strokes = [];

    public CanvasShowcaseWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(235, 235, 235);

        Add(new ColourGraphic(Colour.White, 0f, CanvasTop, Window.Width, Window.Height - CanvasTop));

        const float ToolbarY = 8f;
        const float ToolbarButtonHeight = 30f;

        Add(new TextGraphic(s_font, "Pen:", Colour.DimGray, 10f, ToolbarY, 40f, ToolbarButtonHeight)
        {
            VAlignment = VAlignment.Centred,
        });

        Button penMinus = new(54f, ToolbarY, 28f, ToolbarButtonHeight, ButtonTheme.FromColour(new Colour(180, 90, 90)), "-");
        penMinus.LeftClicked += (_, _) => AdjustPenSize(-PenSizeStep);
        Add(penMinus);

        _penSizeLabel = new TextGraphic(s_font, FormatPenSize(), Colour.DimGray, 86f, ToolbarY, 32f, ToolbarButtonHeight)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_penSizeLabel);

        Button penPlus = new(122f, ToolbarY, 28f, ToolbarButtonHeight, ButtonTheme.FromColour(new Colour(70, 150, 70)), "+");
        penPlus.LeftClicked += (_, _) => AdjustPenSize(+PenSizeStep);
        Add(penPlus);

        const float SwatchSize = 26f;
        const float SwatchPitch = 30f;
        const float SwatchesStartX = 170f;
        float swatchY = ToolbarY + (ToolbarButtonHeight - SwatchSize) / 2f;

        _selectionHighlight = new BorderedRectangleGraphic(0f, 0f, SwatchSize, SwatchSize, 3f, Colour.DimGray, BorderPosition.Outside);
        for (int swatchIndex = 0; swatchIndex < s_palette.Length; swatchIndex++)
        {
            float swatchX = SwatchesStartX + swatchIndex * SwatchPitch;
            Add(CreateColourSwatch(swatchX, swatchY, SwatchSize, s_palette[swatchIndex]));
        }
        Add(_selectionHighlight);
        SelectColour(Colour.Black, SwatchesStartX + SwatchPitch, swatchY); // index 1 in s_palette

        float undoX = SwatchesStartX + s_palette.Length * SwatchPitch + 14f;
        Button undo = new(undoX, ToolbarY, 70f, ToolbarButtonHeight, ButtonTheme.FromColour(new Colour(120, 120, 160)), "Undo");
        undo.LeftClicked += (_, _) => Undo();
        Add(undo);

        Button clear = new(undoX + 78f, ToolbarY, 70f, ToolbarButtonHeight, ButtonTheme.FromColour(new Colour(160, 90, 90)), "Clear");
        clear.LeftClicked += (_, _) => ClearCanvas();
        Add(clear);
    }

    protected override bool BackButtonTopRight => true;

    public override string Name => "Canvas";

    private void AdjustPenSize(float delta)
    {
        _penSize = Math.Clamp(_penSize + delta, MinPenSize, MaxPenSize);
        _penSizeLabel.Text = FormatPenSize();
    }

    private void ClearCanvas()
    {
        foreach (LinePathGraphic stroke in _strokes)
        {
            stroke.Remove();
            stroke.Dispose();
        }
        _strokes.Clear();

        if (_currentStroke is not null)
        {
            _currentStroke.Remove();
            _currentStroke.Dispose();
            _currentStroke = null;
        }
        _strokeStart = null;
    }

    private Button CreateColourSwatch(float x, float y, float size, Colour colour)
    {
        Button swatch = new(x, y, size, size, ButtonTheme.FromColour(colour));
        swatch.LeftClicked += (_, _) => SelectColour(colour, x, y);
        return swatch;
    }

    private void ContinueStroke(Point point)
    {
        if ((point - _lastAppendedPoint).Length < MinPointSpacing)
        {
            return;
        }

        if (_currentStroke is null)
        {
            // Defer creating the graphic until there's genuine movement, so its first segment
            // reflects the real drag direction. Starting from an arbitrary placeholder direction
            // (e.g. a fixed tiny offset) instead would leave that direction baked into the path as
            // a real vertex, producing a sharp, glitchy miter wherever the actual drag disagreed
            // with it.
            _currentStroke = new LinePathGraphic([_strokeStart!.Value, point], _currentColour, _penSize);
            Add(_currentStroke);
        }
        else
        {
            _currentStroke.AppendPoint(point);
        }

        _lastAppendedPoint = point;
    }

    private void EndStroke()
    {
        if (_currentStroke is not null)
        {
            _strokes.Add(_currentStroke);
            _currentStroke = null;
        }

        _strokeStart = null;
    }

    private string FormatPenSize() => $"{_penSize:0}px";

    private void SelectColour(Colour colour, float swatchX, float swatchY)
    {
        _currentColour = colour;
        _selectionHighlight.X = swatchX;
        _selectionHighlight.Y = swatchY;
    }

    private void Undo()
    {
        if (_strokes.Count == 0)
        {
            return;
        }

        LinePathGraphic last = _strokes[^1];
        _strokes.RemoveAt(_strokes.Count - 1);
        last.Remove();
        last.Dispose();
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        Point mousePosition = new(Mouse.ClientX, Mouse.ClientY);

        if (Mouse.LeftPressed && mousePosition.Y >= CanvasTop)
        {
            _strokeStart = mousePosition;
            _lastAppendedPoint = mousePosition;
        }
        else if (_strokeStart is not null)
        {
            if (Mouse.LeftDown)
            {
                // Clamp to the canvas so a stroke dragged up over the toolbar doesn't paint on it.
                Point clamped = new(mousePosition.X, Math.Max(mousePosition.Y, CanvasTop));
                ContinueStroke(clamped);
            }
            else
            {
                EndStroke();
            }
        }
    }
}
