using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class TextDemoWorld2 : DemoWorld
{
    private static readonly FontDefinition[] Fonts =
    [
        new("Times New Roman", 12),
        new("Times New Roman", 16),
        new("Times New Roman", 22),
        new("Times New Roman", 14, FontStyle.Bold),
        new("Times New Roman", 14, FontStyle.Italic),
        new("Times New Roman", 14, FontStyle.BoldItalic),
        new("Courier New", 14),
        new("Arial", 14),
        new("Arial", 18, FontStyle.Bold),
    ];

    private const int BoxX = 100;
    private const int BoxY = 65;
    private const int BoxW = 600;
    private const int BoxH = 220;
    private const int Padding = 6;
    private const int InfoY = BoxY + BoxH + 8;
    private const int ButtonY = InfoY + 24;

    private const string SampleText =
        "The quick brown fox jumps over the lazy dog. " +
        "Pack my box with five dozen liquor jugs. " +
        "Sphinx of black quartz — judge my vow.";

    private int _fontIndex = 0;
    private readonly TextGraphic _sample;
    private readonly TextGraphic _fontInfo;

    public override string Name => "Text Demo 2";

    public TextDemoWorld2(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(180, 180, 180);

        Add(new ColourGraphic(Colour.White, BoxX, BoxY, BoxW, BoxH));

        _sample = new TextGraphic(Fonts[0], SampleText, Colour.Black,
            BoxX + Padding, BoxY + Padding, BoxW - 2 * Padding, BoxH - 2 * Padding)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
        };
        Add(_sample);

        _fontInfo = new TextGraphic(new FontDefinition("Times New Roman", 13), BuildInfoText(), Colour.Black,
            BoxX, InfoY, BoxW, 20)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_fontInfo);

        Button prev = new(BoxX, ButtonY, 130, 34, ButtonTheme.FromColour(new Colour(100, 160, 255)), "← Prev Font");
        prev.LeftClicked += (_, _) => CycleFont(-1);
        Add(prev);

        Button next = new(BoxX + 140, ButtonY, 130, 34, ButtonTheme.FromColour(new Colour(100, 160, 255)), "Next Font →");
        next.LeftClicked += (_, _) => CycleFont(1);
        Add(next);
    }

    private void CycleFont(int direction)
    {
        _fontIndex = (_fontIndex + direction + Fonts.Length) % Fonts.Length;
        _sample.Font = Fonts[_fontIndex];
        _fontInfo.Text = BuildInfoText();
    }

    private string BuildInfoText()
    {
        FontDefinition f = Fonts[_fontIndex];
        string style = f.FontStyle == FontStyle.Regular ? "" : $" {f.FontStyle}";
        return $"Font: {f.FontName} {f.FontSize}pt{style}   ({_fontIndex + 1} / {Fonts.Length})";
    }
}
