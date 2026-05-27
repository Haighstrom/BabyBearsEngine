using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

/// <summary>
/// Visual regression check for text rendering quality. Shows the same sample text rendered
/// at a range of font sizes on both light and dark backgrounds, making AA quality, hinting,
/// and filter artefacts easy to spot by eye.
/// </summary>
internal class TextRenderingQualityDemoWorld : DemoWorld
{
    private static readonly (int Size, string Label)[] s_sizes =
    [
        (9,  "9pt"),
        (11, "11pt"),
        (13, "13pt"),
        (16, "16pt"),
        (20, "20pt"),
        (28, "28pt"),
    ];

    private const string SampleText = "Quick brown fox — 0123 AaBbCc";
    private const string FontName = "Arial";

    public override string Name => "Text Rendering Quality";

    public TextRenderingQualityDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(245, 245, 245);

        Add(new TextGraphic(
            new FontDefinition(FontName, 11),
            "Text rendering quality — light background (top) / dark background (bottom)",
            Colour.Black, 10, 55, 780, 20)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        int lightY = 82;
        int darkPanelY = lightY + s_sizes.Length * 36 + 16;

        Add(new ColourGraphic(new Colour(30, 30, 40), 0, darkPanelY, 800, s_sizes.Length * 36 + 8));

        for (int i = 0; i < s_sizes.Length; i++)
        {
            var (size, label) = s_sizes[i];
            var font = new FontDefinition(FontName, size);

            int lightRowY = lightY + i * 36;
            int darkRowY = darkPanelY + 4 + i * 36;

            Add(new TextGraphic(new FontDefinition(FontName, 10), label, new Colour(100, 100, 100), 10, lightRowY + 10, 40, 20)
            {
                HAlignment = HAlignment.Right,
                VAlignment = VAlignment.Centred,
            });

            Add(new TextGraphic(font, SampleText, Colour.Black, 60, lightRowY + 4, 720, size + 8)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            });

            Add(new TextGraphic(new FontDefinition(FontName, 10), label, new Colour(160, 160, 160), 10, darkRowY + 10, 40, 20)
            {
                HAlignment = HAlignment.Right,
                VAlignment = VAlignment.Centred,
            });

            Add(new TextGraphic(font, SampleText, Colour.White, 60, darkRowY + 4, 720, size + 8)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            });
        }
    }
}
