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
}
