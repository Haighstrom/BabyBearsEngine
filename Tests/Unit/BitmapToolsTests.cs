using System.Drawing;
using System.Drawing.Imaging;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Tests run on Windows.")]
public class BitmapToolsTests
{
    [TestMethod]
    public void PremultiplyAlpha_HalfAlphaPixel_ScalesRgbByAlpha()
    {
        Bitmap bitmap = new(1, 1, PixelFormat.Format32bppArgb);
        bitmap.SetPixel(0, 0, Color.FromArgb(alpha: 128, red: 200, green: 100, blue: 50));

        BitmapTools.PremultiplyAlpha(bitmap);

        Color pixel = bitmap.GetPixel(0, 0);
        // 128/255 * channel, rounded:
        // 200 * 128/255 ≈ 100.39 → 100
        // 100 * 128/255 ≈ 50.20  → 50
        //  50 * 128/255 ≈ 25.10  → 25
        Assert.AreEqual(128, (int)pixel.A);
        Assert.AreEqual(100, (int)pixel.R);
        Assert.AreEqual(50, (int)pixel.G);
        Assert.AreEqual(25, (int)pixel.B);
    }

    [TestMethod]
    public void PremultiplyAlpha_FullyOpaquePixel_LeavesRgbUnchanged()
    {
        Bitmap bitmap = new(1, 1, PixelFormat.Format32bppArgb);
        bitmap.SetPixel(0, 0, Color.FromArgb(alpha: 255, red: 200, green: 100, blue: 50));

        BitmapTools.PremultiplyAlpha(bitmap);

        Color pixel = bitmap.GetPixel(0, 0);
        Assert.AreEqual(255, (int)pixel.A);
        Assert.AreEqual(200, (int)pixel.R);
        Assert.AreEqual(100, (int)pixel.G);
        Assert.AreEqual(50, (int)pixel.B);
    }

    [TestMethod]
    public void PremultiplyAlpha_FullyTransparentPixel_ZerosRgb()
    {
        Bitmap bitmap = new(1, 1, PixelFormat.Format32bppArgb);
        bitmap.SetPixel(0, 0, Color.FromArgb(alpha: 0, red: 200, green: 100, blue: 50));

        BitmapTools.PremultiplyAlpha(bitmap);

        Color pixel = bitmap.GetPixel(0, 0);
        Assert.AreEqual(0, (int)pixel.A);
        Assert.AreEqual(0, (int)pixel.R);
        Assert.AreEqual(0, (int)pixel.G);
        Assert.AreEqual(0, (int)pixel.B);
    }

    [TestMethod]
    public void PremultiplyAlpha_NonArgbFormat_Throws()
    {
        Bitmap bitmap = new(1, 1, PixelFormat.Format24bppRgb);

        Assert.ThrowsExactly<InvalidOperationException>(() => BitmapTools.PremultiplyAlpha(bitmap));
    }
}
