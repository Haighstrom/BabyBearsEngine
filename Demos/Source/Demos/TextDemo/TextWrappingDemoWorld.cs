using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class TextWrappingDemoWorld : DemoWorld
{
    private const int Margin = 20;
    private const int ColGap = 20;
    private const int ColW = 240;
    private const int Col1X = Margin;
    private const int Col2X = Col1X + ColW + ColGap;
    private const int Col3X = Col2X + ColW + ColGap;
    private const int LabelH = 16;
    private const int BoxH = 150;
    private const int Padding = 4;
    private const int Row1Y = 55;
    private const int Row2Y = Row1Y + LabelH + BoxH + 30;

    private const string ParagraphText =
        "The quick brown fox jumps over the lazy dog. Pack my box with five dozen liquor jugs.";

    public override string Name => "Text Wrapping";

    public TextWrappingDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(180, 180, 180);

        AddSection(Col1X, Row1Y, "Word wrap — left",
            MakeMultilineBox(Col1X, Row1Y, ParagraphText, HAlignment.Left));
        AddSection(Col2X, Row1Y, "Word wrap — centred",
            MakeMultilineBox(Col2X, Row1Y, ParagraphText, HAlignment.Centred));
        AddSection(Col3X, Row1Y, "Word wrap — right",
            MakeMultilineBox(Col3X, Row1Y, ParagraphText, HAlignment.Right));

        AddSection(Col1X, Row2Y, "Explicit newlines",
            MakeMultilineBox(Col1X, Row2Y,
                "First line\nSecond line\nThird line\n\nAfter blank line",
                HAlignment.Left));
        AddSection(Col2X, Row2Y, "Character wrap",
            MakeMultilineBox(Col2X, Row2Y,
                "Pneumonoultramicroscopicsilicovolcanoconiosis is a word too long to fit on one line.",
                HAlignment.Left));
        AddSection(Col3X, Row2Y, "Single line (no wrap)",
            MakeSingleLineBox(Col3X, Row2Y));
    }

    private void AddSection(float x, float y, string label, TextGraphic textBox)
    {
        Add(new TextGraphic(new FontDefinition("Times New Roman", 12), label, Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, BoxH));
        Add(textBox);
    }

    private static TextGraphic MakeMultilineBox(float x, float y, string text, HAlignment hAlignment)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 14), text, Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, BoxH - 2 * Padding)
        {
            Multiline = true,
            HAlignment = hAlignment,
            VAlignment = VAlignment.Top,
        };
    }

    private static TextGraphic MakeSingleLineBox(float x, float y)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 14),
            "This long sentence will not wrap even though it exceeds the box width.",
            Colour.Black, x + Padding, y + LabelH + Padding, ColW - 2 * Padding, BoxH - 2 * Padding)
        {
            Multiline = false,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }
}
