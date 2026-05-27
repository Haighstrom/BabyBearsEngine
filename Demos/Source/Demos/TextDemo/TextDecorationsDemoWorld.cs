using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class TextDecorationsDemoWorld : DemoWorld
{
    private const int Margin = 20;
    private const int ColGap = 20;
    private const int ColW = 240;
    private const int Col1X = Margin;
    private const int Col2X = Col1X + ColW + ColGap;
    private const int Col3X = Col2X + ColW + ColGap;
    private const int LabelH = 16;
    private const int SingleBoxH = 80;
    private const int MultiBoxH = 150;
    private const int Padding = 4;
    private const int Row1Y = 70;
    private const int Row2Y = Row1Y + LabelH + SingleBoxH + 24;

    private const string ShortText = "The quick brown fox jumps.";
    private const string LongText =
        "The quick brown fox jumps over the lazy dog. Pack my box with five dozen liquor jugs.";

    public override string Name => "Text Decorations";

    public TextDecorationsDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(180, 180, 180);

        AddSection(Col1X, Row1Y, SingleBoxH, "Underline",
            MakeBox(Col1X, Row1Y, SingleBoxH, ShortText, multiline: false,
                underline: new TextDecoration(Colour.Black), strikethrough: null));
        AddSection(Col2X, Row1Y, SingleBoxH, "Strikethrough",
            MakeBox(Col2X, Row1Y, SingleBoxH, ShortText, multiline: false,
                underline: null, strikethrough: new TextDecoration(Colour.Black)));
        AddSection(Col3X, Row1Y, SingleBoxH, "Underline + strikethrough",
            MakeBox(Col3X, Row1Y, SingleBoxH, ShortText, multiline: false,
                underline: new TextDecoration(Colour.Black), strikethrough: new TextDecoration(Colour.Red, 2f)));

        AddSection(Col1X, Row2Y, MultiBoxH, "Underline (multi-line)",
            MakeBox(Col1X, Row2Y, MultiBoxH, LongText, multiline: true,
                underline: new TextDecoration(Colour.Black), strikethrough: null));
        AddSection(Col2X, Row2Y, MultiBoxH, "Strikethrough (multi-line)",
            MakeBox(Col2X, Row2Y, MultiBoxH, LongText, multiline: true,
                underline: null, strikethrough: new TextDecoration(Colour.Black)));
        AddSection(Col3X, Row2Y, MultiBoxH, "Both (multi-line)",
            MakeBox(Col3X, Row2Y, MultiBoxH, LongText, multiline: true,
                underline: new TextDecoration(Colour.Black), strikethrough: new TextDecoration(Colour.Red, 2f)));
    }

    private void AddSection(float x, float y, int boxH, string label, TextGraphic textBox)
    {
        Add(new TextGraphic(new FontDefinition("Times New Roman", 12), label, Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, boxH));
        Add(textBox);
    }

    private static TextGraphic MakeBox(float x, float y, int boxH, string text, bool multiline,
        TextDecoration? underline, TextDecoration? strikethrough)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 14), text, Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, boxH - 2 * Padding)
        {
            Multiline = multiline,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
            Underline = underline,
            Strikethrough = strikethrough,
        };
    }
}
