using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

/// <summary>
/// Visual comparison of the three text backends side by side. Each row renders the same sample at
/// the same size with GDI+ (left), FreeType (middle) and SDF (right), so the trade-offs are easy to
/// see by eye: GDI and FreeType are both crisp at small fixed sizes (hinted) — GDI is Windows-only,
/// FreeType is the cross-platform counterpart — while SDF is softer when small but scales cleanly.
/// The columns are driven entirely by <see cref="FontDefinition.Renderer"/>, demonstrating the
/// hybrid per-font backend selection — one world, all three backends at once.
/// </summary>
internal class TextRenderingQualityDemoWorld : DemoWorld
{
    private static readonly (int Size, string Label)[] s_sizes =
    [
        (7,  "7pt"),
        (9,  "9pt"),
        (11, "11pt"),
        (13, "13pt"),
        (16, "16pt"),
        (20, "20pt"),
    ];

    // Short enough to fit a single narrow column at the largest size, while still exercising
    // lower-case, upper-case and digit glyphs.
    private const string SampleText = "Quick fox 0123";
    private const string FontName = "Arial";

    private const int LabelX = 6;
    private const int LabelWidth = 30;
    private const int GdiColumnX = 40;
    private const int FreeTypeColumnX = 292;
    private const int SdfColumnX = 544;
    private const int ColumnWidth = 244;
    private const int RowHeight = 36;
    private const int HeaderRowHeight = 26;

    // The header and comparison columns start at the very top-left, so move the Back button out of
    // their way to the top-right corner (which is otherwise empty).
    protected override bool BackButtonTopRight => true;

    public override string Name => "Text Rendering Quality";

    public TextRenderingQualityDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(245, 245, 245);

        Add(new TextGraphic(
            GdiFont(11),
            "Hybrid text backends — GDI & FreeType (hinted) vs SDF (scalable), same sizes",
            Colour.Black, 10, 8, 780, 20)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        int sectionHeight = HeaderRowHeight + s_sizes.Length * RowHeight;

        int lightTop = 40;
        AddComparisonSection(lightTop, Colour.Black, new Colour(100, 100, 100));

        int darkTop = lightTop + sectionHeight + 16;
        Add(new ColourGraphic(new Colour(30, 30, 40), 0, darkTop - 4, 800, sectionHeight + 8));
        AddComparisonSection(darkTop, Colour.White, new Colour(170, 170, 170));
    }

    // Labels and headers are chrome, not the thing under test — render them with GDI so they stay
    // crisp regardless of which backend the row samples are demonstrating.
    private static FontDefinition GdiFont(int size) => new(FontName, size, Renderer: TextRenderer.Gdi);

    private void AddComparisonSection(int top, Colour sampleColour, Colour labelColour)
    {
        Add(new TextGraphic(GdiFont(12), "GDI — hinted", labelColour, GdiColumnX, top, ColumnWidth, 20)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Add(new TextGraphic(GdiFont(12), "FreeType — hinted", labelColour, FreeTypeColumnX, top, ColumnWidth, 20)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Add(new TextGraphic(GdiFont(12), "SDF — scalable", labelColour, SdfColumnX, top, ColumnWidth, 20)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        int rowsTop = top + HeaderRowHeight;

        for (int i = 0; i < s_sizes.Length; i++)
        {
            var (size, label) = s_sizes[i];
            int rowY = rowsTop + i * RowHeight;

            Add(new TextGraphic(GdiFont(10), label, labelColour, LabelX, rowY, LabelWidth, RowHeight)
            {
                HAlignment = HAlignment.Right,
                VAlignment = VAlignment.Centred,
            });

            Add(new TextGraphic(
                new FontDefinition(FontName, size, Renderer: TextRenderer.Gdi),
                SampleText, sampleColour, GdiColumnX, rowY, ColumnWidth, RowHeight)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            });

            Add(new TextGraphic(
                new FontDefinition(FontName, size, Renderer: TextRenderer.FreeType),
                SampleText, sampleColour, FreeTypeColumnX, rowY, ColumnWidth, RowHeight)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            });

            Add(new TextGraphic(
                new FontDefinition(FontName, size, Renderer: TextRenderer.Sdf),
                SampleText, sampleColour, SdfColumnX, rowY, ColumnWidth, RowHeight)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            });
        }
    }
}
