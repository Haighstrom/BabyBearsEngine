using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TextGraphicTests
{

    [TestMethod]
    public void ShouldPixelSnap_NativeScaleNoRotation_ReturnsTrue()
    {
        Assert.IsTrue(TextGraphic.ShouldPixelSnap(scaleX: 1f, scaleY: 1f, angle: 0f));
    }

    [TestMethod]
    public void ShouldPixelSnap_ScaleWithinTolerance_ReturnsTrue()
    {
        // A camera tile size that divides to very-nearly 1 should still snap — the tolerance
        // absorbs floating-point drift so native-size text isn't left un-snapped.
        Assert.IsTrue(TextGraphic.ShouldPixelSnap(scaleX: 0.9999f, scaleY: 1.0001f, angle: 0f));
    }

    [TestMethod]
    public void ShouldPixelSnap_ScaledUp_ReturnsFalse()
    {
        Assert.IsFalse(TextGraphic.ShouldPixelSnap(scaleX: 2f, scaleY: 2f, angle: 0f));
    }

    [TestMethod]
    public void ShouldPixelSnap_ScaledDown_ReturnsFalse()
    {
        Assert.IsFalse(TextGraphic.ShouldPixelSnap(scaleX: 0.5f, scaleY: 0.5f, angle: 0f));
    }

    [TestMethod]
    public void ShouldPixelSnap_OnlyOneAxisScaled_ReturnsFalse()
    {
        // Both axes must be at native scale; snapping when only one is would distort the glyph.
        Assert.IsFalse(TextGraphic.ShouldPixelSnap(scaleX: 1f, scaleY: 1.5f, angle: 0f));
    }

    [TestMethod]
    public void ShouldPixelSnap_Rotated_ReturnsFalse()
    {
        // A rotated quad's corners don't land on the pixel grid, so snapping would warp it.
        Assert.IsFalse(TextGraphic.ShouldPixelSnap(scaleX: 1f, scaleY: 1f, angle: 0.5f));
    }

    [TestMethod]
    public void IsContentTruncated_CellFullyInside_ReturnsFalse()
    {
        // A glyph whose advance cell sits comfortably within the bounds is not truncated, even
        // though its SDF render quad may spill past the edges with transparent glow — the regression
        // this guards: glow margins must not be mistaken for clipped text.
        Assert.IsFalse(TextGraphic.IsContentTruncated(
            charLeft: 10f, charAdvance: 12f, lineTop: 4f, lineHeight: 20f, width: 360f, height: 36f));
    }

    [TestMethod]
    public void IsContentTruncated_CellTouchingFarEdges_ReturnsFalse()
    {
        // Content that reaches exactly to the right/bottom edge fits — the test is strictly
        // greater-than, so a cell ending on the boundary is not flagged.
        Assert.IsFalse(TextGraphic.IsContentTruncated(
            charLeft: 0f, charAdvance: 360f, lineTop: 0f, lineHeight: 36f, width: 360f, height: 36f));
    }

    [TestMethod]
    public void IsContentTruncated_NegativeCharLeft_ReturnsTrue()
    {
        // Right- or centre-aligned text wider than the box starts at a negative x — genuinely
        // clipped on the left.
        Assert.IsTrue(TextGraphic.IsContentTruncated(
            charLeft: -5f, charAdvance: 12f, lineTop: 4f, lineHeight: 20f, width: 360f, height: 36f));
    }

    [TestMethod]
    public void IsContentTruncated_CellPastRightEdge_ReturnsTrue()
    {
        // The advance cell extends beyond the right edge: the glyph is cut off horizontally.
        Assert.IsTrue(TextGraphic.IsContentTruncated(
            charLeft: 355f, charAdvance: 12f, lineTop: 4f, lineHeight: 20f, width: 360f, height: 36f));
    }

    [TestMethod]
    public void IsContentTruncated_NegativeLineTop_ReturnsTrue()
    {
        // Bottom- or centre-aligned text taller than the box starts above the top edge.
        Assert.IsTrue(TextGraphic.IsContentTruncated(
            charLeft: 10f, charAdvance: 12f, lineTop: -2f, lineHeight: 20f, width: 360f, height: 36f));
    }

    [TestMethod]
    public void IsContentTruncated_LinePastBottomEdge_ReturnsTrue()
    {
        // The line box extends below the bottom edge: the row is cut off vertically.
        Assert.IsTrue(TextGraphic.IsContentTruncated(
            charLeft: 10f, charAdvance: 12f, lineTop: 30f, lineHeight: 20f, width: 360f, height: 36f));
    }

    // Bold/italic auto-discovery convention. Exercised via the internal helper so the tests do
    // not need a GL context (TextGraphic's decoration shader Lazy<> would fail to initialise).

    [TestMethod]
    public void TryAutoDiscoverVariant_FontWithBoldCompanion_ReturnsRenamedFontDefinition()
    {
        // Arial_b.ttf is shipped in Assets/Fonts as the bold companion to Arial.ttf.
        FontDefinition baseFont = new("Arial", 12f);

        FontDefinition? bold = TextGraphic.TryAutoDiscoverVariant(baseFont, "_b");

        Assert.IsNotNull(bold);
        Assert.AreEqual("Arial_b", bold!.FontName);
        Assert.AreEqual(12f, bold.FontSize);
    }

    [TestMethod]
    public void TryAutoDiscoverVariant_FontWithItalicCompanion_ReturnsRenamedFontDefinition()
    {
        FontDefinition baseFont = new("Arial", 12f);

        FontDefinition? italic = TextGraphic.TryAutoDiscoverVariant(baseFont, "_i");

        Assert.IsNotNull(italic);
        Assert.AreEqual("Arial_i", italic!.FontName);
    }

    [TestMethod]
    public void TryAutoDiscoverVariant_FontWithBoldItalicCompanion_ReturnsRenamedFontDefinition()
    {
        // Arial_bi.ttf is shipped in Assets/Fonts as the bold-italic companion to Arial.ttf.
        FontDefinition baseFont = new("Arial", 12f);

        FontDefinition? boldItalic = TextGraphic.TryAutoDiscoverVariant(baseFont, "_bi");

        Assert.IsNotNull(boldItalic);
        Assert.AreEqual("Arial_bi", boldItalic!.FontName);
    }

    [TestMethod]
    public void TryAutoDiscoverVariant_TahomaHasNoBoldItalicCompanion_ReturnsNull()
    {
        // Tahoma ships no italic at all on Windows, so neither _i nor _bi exist.
        FontDefinition baseFont = new("Tahoma", 12f);

        Assert.IsNull(TextGraphic.TryAutoDiscoverVariant(baseFont, "_bi"));
    }

    [TestMethod]
    public void TryAutoDiscoverVariant_TahomaHasBoldButNoItalic_ReturnsBoldNotItalic()
    {
        // Windows ships Tahoma Bold but no Tahoma Italic — the convention should distinguish.
        FontDefinition baseFont = new("Tahoma", 12f);

        Assert.IsNotNull(TextGraphic.TryAutoDiscoverVariant(baseFont, "_b"));
        Assert.IsNull(TextGraphic.TryAutoDiscoverVariant(baseFont, "_i"));
    }

    [TestMethod]
    public void TryAutoDiscoverVariant_FontWithNoCompanion_ReturnsNull()
    {
        FontDefinition baseFont = new("MadeUpFont", 12f);

        Assert.IsNull(TextGraphic.TryAutoDiscoverVariant(baseFont, "_b"));
        Assert.IsNull(TextGraphic.TryAutoDiscoverVariant(baseFont, "_i"));
    }

    [TestMethod]
    public void TryAutoDiscoverVariant_PreservesSizeRendererAndExtraChars()
    {
        // The variant must inherit everything but the FontName — size, renderer, and the
        // ExtraCharactersToLoad list — so its atlas covers the same glyph set as the base.
        FontDefinition baseFont = new("Arial", 18f, ExtraCharactersToLoad: "αβ", Renderer: TextRenderer.Gdi);

        FontDefinition? bold = TextGraphic.TryAutoDiscoverVariant(baseFont, "_b");

        Assert.IsNotNull(bold);
        Assert.AreEqual(18f, bold!.FontSize);
        Assert.AreEqual(TextRenderer.Gdi, bold.Renderer);
        Assert.AreEqual("αβ", bold.ExtraCharactersToLoad);
    }
}
