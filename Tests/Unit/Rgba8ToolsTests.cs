using System;
using BabyBearsEngine.Platform.ImageLoading;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Rgba8ToolsTests
{
    [TestMethod]
    public void PremultiplyAlphaInPlace_OpaquePixel_Unchanged()
    {
        byte[] data = [100, 150, 200, 255];

        Rgba8Tools.PremultiplyAlphaInPlace(data);

        CollectionAssert.AreEqual(new byte[] { 100, 150, 200, 255 }, data);
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_FullyTransparentPixel_RgbBecomesZero()
    {
        byte[] data = [200, 150, 100, 0];

        Rgba8Tools.PremultiplyAlphaInPlace(data);

        CollectionAssert.AreEqual(new byte[] { 0, 0, 0, 0 }, data);
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_HalfTransparentPixel_RgbApproximatelyHalved()
    {
        // alpha = 128 → rgb * 128 / 255 (truncated)
        // 200 * 128 / 255 = 25600 / 255 = 100
        // 100 * 128 / 255 = 12800 / 255 = 50
        // 50  * 128 / 255 = 6400  / 255 = 25
        byte[] data = [200, 100, 50, 128];

        Rgba8Tools.PremultiplyAlphaInPlace(data);

        CollectionAssert.AreEqual(new byte[] { 100, 50, 25, 128 }, data);
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_WhitePixelAtVariousAlphas_RgbEqualsAlpha()
    {
        // (255, 255, 255, A) premultiplies to (A, A, A, A)
        byte[] data = [255, 255, 255, 0, 255, 255, 255, 64, 255, 255, 255, 128, 255, 255, 255, 255];

        Rgba8Tools.PremultiplyAlphaInPlace(data);

        CollectionAssert.AreEqual(new byte[]
        {
            0, 0, 0, 0,
            64, 64, 64, 64,
            128, 128, 128, 128,
            255, 255, 255, 255,
        }, data);
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_MultiplePixels_EachIndependentlyPremultiplied()
    {
        byte[] data =
        [
            100, 150, 200, 255,
            100, 150, 200, 0,
            100, 150, 200, 128,
        ];

        Rgba8Tools.PremultiplyAlphaInPlace(data);

        CollectionAssert.AreEqual(new byte[]
        {
            100, 150, 200, 255,
            0, 0, 0, 0,
            50, 75, 100, 128,
        }, data);
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_EmptyBuffer_NoOp()
    {
        byte[] data = [];

        Rgba8Tools.PremultiplyAlphaInPlace(data);

        Assert.IsEmpty(data);
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_NullBuffer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Rgba8Tools.PremultiplyAlphaInPlace(null!));
    }

    [TestMethod]
    public void PremultiplyAlphaInPlace_LengthNotMultipleOfFour_Throws()
    {
        byte[] data = [100, 150, 200];

        Assert.ThrowsExactly<ArgumentException>(() => Rgba8Tools.PremultiplyAlphaInPlace(data));
    }
}
