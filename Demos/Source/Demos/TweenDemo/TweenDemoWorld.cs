using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Demos.Source.Demos.TweenDemo;

/// <summary>
/// Shows all built-in easing functions side-by-side as animated bars, plus a ColourTween row.
/// Each bar travels from left to right over <see cref="Duration"/> seconds and loops.
/// </summary>
internal class TweenDemoWorld : DemoWorld
{
    private const float LabelX    = 5f;
    private const float LabelW    = 168f;
    private const float TrackX    = 178f;
    private const float BarMinX   = 178f;
    private const float BarMaxX   = 730f;
    private const float BarW      = 50f;
    private const float BarH      = 26f;
    private const float RowH      = 40f;
    private const float FirstRowY = 50f;
    private const double Duration = 3.0;

    private static readonly FontDefinition s_font      = new("Times New Roman", 13);
    private static readonly Colour         s_barColour = new(60, 130, 220);
    private static readonly Colour         s_trackColour = new(160, 160, 160);

    private readonly List<(NumTween Tween, ColourGraphic Bar)> _rows = [];
    private ColourTween _colourTween = null!;
    private ColourGraphic _colourBar = null!;

    public override string Name => "Tween Demo";

    public TweenDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(210, 210, 210);

        AddPositionRow(0, "Linear",        null);
        AddPositionRow(1, "EaseInQuad",    Easings.EaseInQuad);
        AddPositionRow(2, "EaseOutQuad",   Easings.EaseOutQuad);
        AddPositionRow(3, "EaseInOutQuad", Easings.EaseInOutQuad);
        AddPositionRow(4, "EaseInSine",    Easings.EaseInSine);
        AddPositionRow(5, "EaseOutBounce", Easings.EaseOutBounce);
        AddPositionRow(6, "EaseInBack",    Easings.EaseInBack);
        AddPositionRow(7, "EaseOutBack",   Easings.EaseOutBack);

        AddColourRow();
    }

    private void AddPositionRow(int rowIndex, string label, Func<double, double>? easing)
    {
        float y = FirstRowY + rowIndex * RowH;

        Add(new TextGraphic(s_font, label, Colour.Black, LabelX, y, LabelW, BarH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        // Thin track shows the full travel range behind the bar.
        Add(new ColourGraphic(s_trackColour, TrackX, y + BarH / 2f - 1f, BarMaxX - TrackX + BarW, 2f));

        ColourGraphic bar = new(s_barColour, BarMinX, y, BarW, BarH);
        NumTween tween = new(BarMinX, BarMaxX, Duration, loop: true, easing: easing);

        Add(bar);
        Add(tween);
        _rows.Add((tween, bar));
    }

    private void AddColourRow()
    {
        float y = FirstRowY + 8 * RowH + 20f;

        Add(new TextGraphic(s_font, "ColourTween  (EaseInOutSine)", Colour.Black, LabelX, y, LabelW + 30f, BarH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Colour fromColour = new(220, 80, 60);
        Colour toColour   = new(60, 120, 220);

        _colourBar   = new ColourGraphic(fromColour, TrackX, y, BarMaxX - TrackX + BarW, BarH);
        _colourTween = new ColourTween(fromColour, toColour, Duration, loop: true, easing: Easings.EaseInOutSine);

        Add(_colourBar);
        Add(_colourTween);
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        foreach (var (tween, bar) in _rows)
        {
            bar.X = (float)tween.Value;
        }

        _colourBar.Colour = _colourTween.Value;
    }
}
