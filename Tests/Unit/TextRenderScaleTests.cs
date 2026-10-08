using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TextRenderScaleTests
{
    [TestMethod]
    public void Compute_WindowTwiceCanvas_ReturnsTwo()
    {
        (float x, float y) = TextRenderScale.Compute(2560, 1440, 1280, 720);

        Assert.AreEqual(2f, x);
        Assert.AreEqual(2f, y);
    }

    [TestMethod]
    public void Compute_WindowSmallerThanCanvas_ReturnsFraction()
    {
        (float x, float y) = TextRenderScale.Compute(960, 540, 1280, 720);

        Assert.AreEqual(0.75f, x);
        Assert.AreEqual(0.75f, y);
    }

    [TestMethod]
    public void Compute_StretchedWindow_ReturnsPerAxisScales()
    {
        (float x, float y) = TextRenderScale.Compute(1920, 720, 1280, 720);

        Assert.AreEqual(1.5f, x);
        Assert.AreEqual(1f, y);
    }

    [TestMethod]
    public void Compute_ExtremeSizes_AreClamped()
    {
        (float tinyX, float tinyY) = TextRenderScale.Compute(10, 10, 1280, 720);
        (float hugeX, float hugeY) = TextRenderScale.Compute(100000, 100000, 1280, 720);

        Assert.AreEqual(0.5f, tinyX);
        Assert.AreEqual(0.5f, tinyY);
        Assert.AreEqual(4f, hugeX);
        Assert.AreEqual(4f, hugeY);
    }
}
