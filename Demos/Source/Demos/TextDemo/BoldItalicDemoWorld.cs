using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class BoldItalicDemoWorld : DemoWorld
{
    private const int Margin   = 20;
    private const int ColGap   = 20;
    private const int ColW     = 240;
    private const int Col1X    = Margin;
    private const int Col2X    = Col1X + ColW + ColGap;
    private const int Col3X    = Col2X + ColW + ColGap;
    private const int LabelH   = 14;
    private const int SmallH   = 45;
    private const int MultiH   = 90;
    private const int RowGap   = 10;
    private const int Padding  = 5;
    private const int Row1Y    = 50;
    private const int Row2Y    = Row1Y + LabelH + SmallH + RowGap;
    private const int Row3Y    = Row2Y + LabelH + SmallH + RowGap;
    private const int Row4Y    = Row3Y + LabelH + MultiH + RowGap;

    // Default base font for most boxes. Bold and italic companions (Times New Roman_b.ttf and
    // Times New Roman_i.ttf) live alongside in Assets/Fonts and are picked up automatically when
    // a TextGraphic is constructed with this definition.
    private static readonly FontDefinition s_font      = new("Times New Roman", 14);
    private static readonly FontDefinition s_labelFont = new("Times New Roman", 11);

    public override string Name => "Bold & Italic";

    public BoldItalicDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(180, 180, 180);

        // ── Row 1: standalone <b>, <i>, and combined <b><i> ─────────────────
        AddSmall(Col1X, Row1Y, "Bold  <b>...</b>",
            "Plain with <b>bold</b> inside.");

        AddSmall(Col2X, Row1Y, "Italic  <i>...</i>",
            "Plain with <i>italic</i> inside.");

        AddSmall(Col3X, Row1Y, "Bold-italic  <b><i>...</i></b>",
            "Plain with <b><i>bold italic</i></b> inside.");

        // ── Row 2: bold/italic combined with the existing tags ──────────────
        AddSmall(Col1X, Row2Y, "Bold + colour",
            "<b><colour=#2266BB>Bold blue heading</colour></b> body.");

        AddSmall(Col2X, Row2Y, "Italic + underline",
            "Citation: <i><u>The Origin of Species</u></i>.");

        AddSmall(Col3X, Row2Y, "Close-last with bold",
            "<b><colour=#CC3333>Both active</> bold only</b> plain.");

        // ── Row 3: multiline — wrapped text mixing bold and italic spans ────
        AddMulti(Col1X, Row3Y, "Multiline — bold spans",
            "The quick <b>brown fox</b> jumps over the <b>lazy dog</b> while the rabbit watches from a distance.");

        AddMulti(Col2X, Row3Y, "Multiline — italic spans",
            "She whispered <i>\"please don't go\"</i> but he was already <i>walking away</i> into the cold night air.");

        AddMulti(Col3X, Row3Y, "Multiline — bold + italic",
            "Elder: <b>The <i>Hearthstone</i> was stolen</b> — only <i>you</i> can recover what was <b>lost</b>.");

        // ── Row 4: variant overrides and the fallback warning ───────────────
        AddOverrideBox(Col1X, Row4Y);
        AddFallbackBox(Col2X, Row4Y);
        AddVariedFontsBox(Col3X, Row4Y);
    }

    // Demonstrates overriding the auto-discovered variants with a different family — here,
    // Consolas Bold and Consolas Bold-Italic — so the styled spans render in a contrasting typeface.
    private void AddOverrideBox(float x, float y)
    {
        Add(new TextGraphic(s_labelFont, "Override: <b>/<bi> = Consolas", Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, MultiH));

        TextGraphic textGraphic = new(s_font,
            "Type <b>EXIT_NOW</b> to quit, or <b><i>HELP_ALL</i></b> for full docs.",
            Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, MultiH - 2 * Padding)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
            BoldFont = new FontDefinition("Consolas_b", 14),
            BoldItalicFont = new FontDefinition("Consolas_bi", 14),
        };
        Add(textGraphic);
    }

    // Tahoma ships no italic on Windows — so the auto-discovery finds Tahoma_b.ttf for bold but
    // leaves ItalicFont and BoldItalicFont null. <i> falls back to the base font, and <b><i>
    // walks the chain: bold-italic → bold → italic → base, here landing on the bold font.
    private void AddFallbackBox(float x, float y)
    {
        Add(new TextGraphic(s_labelFont, "Fallback: Tahoma (no italic shipped)", Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, MultiH));

        FontDefinition tahomaFont = new("Tahoma", 14);

        Add(new TextGraphic(tahomaFont,
            "<b>bold works</b>; <i>italic</i> falls back to base; <b><i>bold-italic</i></b> walks the chain to bold.",
            Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, MultiH - 2 * Padding)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
        });
    }

    // Same string across three different font families — proves the _b / _i auto-discovery
    // works for every font that ships a companion file in Assets/Fonts.
    private void AddVariedFontsBox(float x, float y)
    {
        Add(new TextGraphic(s_labelFont, "Auto-discovery across fonts", Colour.Black, x, y, ColW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Add(new ColourGraphic(Colour.White, x, y + LabelH, ColW, MultiH));

        const string sample = "Plain, <b>bold</b>, <i>italic</i>, <b><i>both</i></b>.";
        float rowH = (MultiH - 2 * Padding) / 3f;

        Add(new TextGraphic(new FontDefinition("Arial", 13), "Arial — " + sample, Colour.Black,
            x + Padding, y + LabelH + Padding, ColW - 2 * Padding, rowH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Add(new TextGraphic(new FontDefinition("Georgia", 13), "Georgia — " + sample, Colour.Black,
            x + Padding, y + LabelH + Padding + rowH, ColW - 2 * Padding, rowH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Add(new TextGraphic(new FontDefinition("Verdana", 13), "Verdana — " + sample, Colour.Black,
            x + Padding, y + LabelH + Padding + 2 * rowH, ColW - 2 * Padding, rowH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
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
