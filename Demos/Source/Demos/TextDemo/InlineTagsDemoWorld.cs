using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class InlineTagsDemoWorld : DemoWorld
{
    private const int Margin   = 20;
    private const int ColGap   = 20;
    private const int ColW     = 240;
    private const int Col1X    = Margin;
    private const int Col2X    = Col1X + ColW + ColGap;
    private const int Col3X    = Col2X + ColW + ColGap;
    private const int WideW    = ColW * 3 + ColGap * 2;
    private const int LabelH   = 14;
    private const int SmallH   = 45;
    private const int MultiH   = 90;
    private const int RowGap   = 10;
    private const int Padding  = 5;
    private const int Row1Y    = 50;
    private const int Row2Y    = Row1Y + LabelH + SmallH + RowGap;
    private const int Row3Y    = Row2Y + LabelH + SmallH + RowGap;
    private const int Row4Y    = Row3Y + LabelH + MultiH + RowGap;

    private static readonly FontDefinition s_font      = new("Times New Roman", 14);
    private static readonly FontDefinition s_labelFont = new("Times New Roman", 11);

    public override string Name => "Inline Tags Demo";

    public InlineTagsDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(180, 180, 180);

        // ── Row 1: colour, underline, strikethrough ──────────────────────────
        AddSmall(Col1X, Row1Y, "Colour override",
            "Normal, <colour=#CC3333>red</colour>, <colour=#2266BB>blue</colour>, <colour=#228833>green</colour>.");

        AddSmall(Col2X, Row1Y, "Underline  <u>...</u>",
            "Text with <u>underline</u> here.");

        AddSmall(Col3X, Row1Y, "Strikethrough  <s>...</s>",
            "Text with <s>strikethrough</s> here.");

        // ── Row 2: combined, nested, close-last ──────────────────────────────
        AddSmall(Col1X, Row2Y, "Colour + underline",
            "<colour=#2266BB><u>Blue underlined link</u></colour> text.");

        AddSmall(Col2X, Row2Y, "Nested colours",
            "<colour=#CC3333>Red <colour=#2266BB>inner blue</colour> red</colour> normal.");

        AddSmall(Col3X, Row2Y, "Close-last tag  \\</> ",
            "<colour=#CC3333><u>Both active</> colour only</colour> normal.");

        // ── Row 3: multiline — each box wraps and uses inline tags ───────────
        AddMulti(Col1X, Row3Y, "Multiline — underline",
            "The quick <u>brown fox</u> jumps over the <u>lazy dog</u> and keeps on running for a while.");

        AddMulti(Col2X, Row3Y, "Multiline — strikethrough",
            "She <s>was frightened</s> and <s>didn't know what to do</s> but then decided to act bravely.");

        AddMulti(Col3X, Row3Y, "Multiline — colour",
            "He found a <colour=#BB8800>golden key</colour>, a <colour=#2266BB>sapphire ring</colour>, and a <colour=#CC3333>ruby gem</colour>.");

        // ── Row 4: wide boxes — game dialogue and UseInlineTags=false ────────
        AddWide(Col1X, Row4Y, "Multiline — mixed styles",
            "Elder: <colour=#774488>The <u>Hearthstone</u> was stolen.</colour> " +
            "<colour=#CC3333><s>All hope</s></colour> may yet be restored — will you seek it out?",
            useInlineTags: true);

        // Right two-thirds: UseInlineTags=false shows raw tag characters
        int twoColW = ColW * 2 + ColGap;
        Add(new TextGraphic(s_labelFont, "UseInlineTags = false  (tags rendered literally)",
            Colour.Black, Col2X, Row4Y, twoColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, Col2X, Row4Y + LabelH, twoColW, MultiH));
        Add(new TextGraphic(s_font,
            "Some <colour=#CC3333>coloured</colour> and <u>underlined</u> text.",
            Colour.Black,
            Col2X + Padding, Row4Y + LabelH + Padding, twoColW - 2 * Padding, MultiH - 2 * Padding)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
            UseInlineTags = false,
        });
    }

    private void AddSmall(float x, float y, string label, string text)
    {
        AddBox(x, y, label, SmallH, false, text);
    }

    private void AddMulti(float x, float y, string label, string text)
    {
        AddBox(x, y, label, MultiH, true, text);
    }

    private void AddWide(float x, float y, string label, string text, bool useInlineTags)
    {
        Add(new TextGraphic(s_labelFont, label, Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, MultiH));
        Add(new TextGraphic(s_font, text, Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, MultiH - 2 * Padding)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
            UseInlineTags = useInlineTags,
        });
    }

    private void AddBox(float x, float y, string label, float boxH, bool multiline, string text)
    {
        Add(new TextGraphic(s_labelFont, label, Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, boxH));
        Add(new TextGraphic(s_font, text, Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, boxH - 2 * Padding)
        {
            Multiline = multiline,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
    }
}
