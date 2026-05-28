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
}
