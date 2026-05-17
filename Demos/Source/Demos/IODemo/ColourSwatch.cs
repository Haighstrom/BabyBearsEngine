using System;

namespace BabyBearsEngine.Demos.Source.Demos.IODemo;

internal class ColourSwatch : Entity
{
    private static readonly Colour[] s_palette =
    [
        Colour.Crimson,
        Colour.OrangeRed,
        Colour.Gold,
        Colour.YellowGreen,
        Colour.ForestGreen,
        Colour.MediumTurquoise,
        Colour.CornflowerBlue,
        Colour.BlueViolet,
        Colour.Violet,
        Colour.HotPink,
        Colour.White,
        Colour.LightGray,
        Colour.DimGray,
        Colour.Black,
    ];

    private readonly ColourGraphic _fill;
    private int _paletteIndex = 0;

    public ColourSwatch(float x, float y, float width, float height, int initialIndex = 0)
        : base(x, y, width, height, clickable: true)
    {
        _paletteIndex = ((initialIndex % s_palette.Length) + s_palette.Length) % s_palette.Length;
        _fill = new ColourGraphic(s_palette[_paletteIndex], 0f, 0f, width, height);
        Add(_fill);
    }

    public Colour Colour
    {
        get => _fill.Colour;
        set
        {
            _fill.Colour = value;
            int idx = Array.IndexOf(s_palette, value);
            _paletteIndex = idx >= 0 ? idx : 0;
        }
    }

    public event EventHandler? ColourChanged;

    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();
        _paletteIndex = (_paletteIndex + 1) % s_palette.Length;
        _fill.Colour = s_palette[_paletteIndex];
        ColourChanged?.Invoke(this, EventArgs.Empty);
    }
}
