using BabyBearsEngine.Platform.OpenTK;

using OpenTKColor4 = OpenTK.Mathematics.Color4;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ColourTests
{
    private const double Delta = 1e-4;

    // Constructors

    [TestMethod]
    public void ByteConstructor_StoresComponents()
    {
        Colour c = new(10, 20, 30, 40);
        Assert.AreEqual((byte)10, c.R);
        Assert.AreEqual((byte)20, c.G);
        Assert.AreEqual((byte)30, c.B);
        Assert.AreEqual((byte)40, c.A);
    }

    [TestMethod]
    public void ByteConstructor_DefaultAlphaIs255()
    {
        Colour c = new(10, 20, 30);
        Assert.AreEqual((byte)255, c.A);
    }

    [TestMethod]
    public void FloatConstructor_RoundsToBytes()
    {
        Colour c = new(0f, 0.5f, 1f, 0.25f);
        Assert.AreEqual((byte)0, c.R);
        Assert.AreEqual((byte)128, c.G);
        Assert.AreEqual((byte)255, c.B);
        Assert.AreEqual((byte)64, c.A);
    }

    [TestMethod]
    public void FloatConstructor_DefaultAlphaIsFullyOpaque()
    {
        Colour c = new(0f, 0f, 0f);
        Assert.AreEqual((byte)255, c.A);
    }

    // Note: out-of-range float clamping is intentionally not directly tested. In DEBUG builds,
    // FloatToByte calls Logger.Log which currently fails due to a pre-existing bug in Logger
    // (creates log.txt as a directory). Restore these tests once Logger is fixed.

    [TestMethod]
    public void CopyByteAlphaConstructor_ReplacesAlpha_PreservesRgb()
    {
        Colour source = new(10, 20, 30, 40);
        Colour result = new(source, (byte)200);
        Assert.AreEqual((byte)10, result.R);
        Assert.AreEqual((byte)20, result.G);
        Assert.AreEqual((byte)30, result.B);
        Assert.AreEqual((byte)200, result.A);
    }

    [TestMethod]
    public void CopyFloatAlphaConstructor_ReplacesAlpha_PreservesRgb()
    {
        Colour source = new(10, 20, 30, 40);
        Colour result = new(source, 1f);
        Assert.AreEqual((byte)10, result.R);
        Assert.AreEqual((byte)20, result.G);
        Assert.AreEqual((byte)30, result.B);
        Assert.AreEqual((byte)255, result.A);
    }

    // Normalised components

    [TestMethod]
    public void NormalisedComponents_MapByteRangeToZeroOne()
    {
        Colour c = new(0, 128, 255, 64);
        Assert.AreEqual(0.0, c.NormalisedR, Delta);
        Assert.AreEqual(128.0 / 255.0, c.NormalisedG, Delta);
        Assert.AreEqual(1.0, c.NormalisedB, Delta);
        Assert.AreEqual(64.0 / 255.0, c.NormalisedA, Delta);
    }

    // Equality

    [TestMethod]
    public void Equality_SameComponents_AreEqual()
    {
        Colour a = new(10, 20, 30, 40);
        Colour b = new(10, 20, 30, 40);
        Assert.IsTrue(a == b);
        Assert.IsTrue(a.Equals(b));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_DifferentRgb_AreNotEqual()
    {
        Colour a = new(10, 20, 30, 40);
        Colour b = new(11, 20, 30, 40);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equality_DifferentAlpha_AreNotEqual()
    {
        Colour a = new(10, 20, 30, 40);
        Colour b = new(10, 20, 30, 41);
        Assert.IsTrue(a != b);
    }

    // WithAlpha

    [TestMethod]
    public void WithAlpha_ReplacesAlpha_PreservesRgb()
    {
        Colour c = new(10, 20, 30, 40);
        Colour result = c.WithAlpha(99);
        Assert.AreEqual((byte)10, result.R);
        Assert.AreEqual((byte)20, result.G);
        Assert.AreEqual((byte)30, result.B);
        Assert.AreEqual((byte)99, result.A);
    }

    [TestMethod]
    public void WithAlpha_DoesNotMutateOriginal()
    {
        Colour c = new(10, 20, 30, 40);
        c.WithAlpha(99);
        Assert.AreEqual((byte)40, c.A);
    }

    // Darkened / Lightened

    [TestMethod]
    public void Darkened_ByZero_LeavesColourUnchanged()
    {
        Colour c = new(120, 80, 40, 200);
        Colour result = c.Darkened(0f);
        Assert.AreEqual(c.R, result.R);
        Assert.AreEqual(c.G, result.G);
        Assert.AreEqual(c.B, result.B);
        Assert.AreEqual(c.A, result.A);
    }

    [TestMethod]
    public void Darkened_ByOne_ProducesBlackRgb()
    {
        Colour c = new(120, 80, 40, 200);
        Colour result = c.Darkened(1f);
        Assert.AreEqual((byte)0, result.R);
        Assert.AreEqual((byte)0, result.G);
        Assert.AreEqual((byte)0, result.B);
    }

    [TestMethod]
    public void Darkened_PreservesAlpha()
    {
        Colour c = new(120, 80, 40, 200);
        Colour result = c.Darkened(0.5f);
        Assert.AreEqual((byte)200, result.A);
    }

    [TestMethod]
    public void Lightened_ByZero_LeavesColourUnchanged()
    {
        Colour c = new(120, 80, 40, 200);
        Colour result = c.Lightened(0f);
        Assert.AreEqual(c.R, result.R);
        Assert.AreEqual(c.G, result.G);
        Assert.AreEqual(c.B, result.B);
        Assert.AreEqual(c.A, result.A);
    }

    [TestMethod]
    public void Lightened_ByOne_ProducesWhiteRgb()
    {
        Colour c = new(120, 80, 40, 200);
        Colour result = c.Lightened(1f);
        Assert.AreEqual((byte)255, result.R);
        Assert.AreEqual((byte)255, result.G);
        Assert.AreEqual((byte)255, result.B);
    }

    [TestMethod]
    public void Lightened_PreservesAlpha()
    {
        Colour c = new(120, 80, 40, 200);
        Colour result = c.Lightened(0.5f);
        Assert.AreEqual((byte)200, result.A);
    }

    // ToArgb

    [TestMethod]
    public void ToArgb_PacksComponentsInArgbOrder()
    {
        Colour c = new(0x12, 0x34, 0x56, 0x78);
        int packed = c.ToArgb();
        Assert.AreEqual(0x78123456, packed);
    }

    // ToColor (System.Drawing)

    [TestMethod]
    public void ToColor_ReturnsSystemDrawingColourWithMatchingComponents()
    {
        Colour c = new(10, 20, 30, 40);
        var sdc = c.ToColor;
        Assert.AreEqual(10, sdc.R);
        Assert.AreEqual(20, sdc.G);
        Assert.AreEqual(30, sdc.B);
        Assert.AreEqual(40, sdc.A);
    }

    // OpenTK round-trip

    [TestMethod]
    public void ToOpenTK_NormalisesByteComponents()
    {
        Colour c = new(0, 128, 255, 64);
        OpenTKColor4 result = c.ToOpenTK();
        Assert.AreEqual(0.0, result.R, Delta);
        Assert.AreEqual(128.0 / 255.0, result.G, Delta);
        Assert.AreEqual(1.0, result.B, Delta);
        Assert.AreEqual(64.0 / 255.0, result.A, Delta);
    }

    [TestMethod]
    public void OpenTKRoundTrip_PreservesByteComponents()
    {
        // Sample across the byte range, including endpoints and a mid-range value
        // that lands on a non-trivial fraction (128/255 = 0.5019...).
        byte[] values = [0, 1, 64, 128, 200, 254, 255];
        foreach (byte v in values)
        {
            Colour original = new(v, v, v, v);
            Colour roundTripped = original.ToOpenTK().ToColour();
            Assert.AreEqual(original, roundTripped, $"Round-trip failed for component value {v}.");
        }
    }

    // Named colour spot checks

    [TestMethod]
    public void White_HasAllByteMaxComponents()
    {
        Colour white = Colour.White;
        Assert.AreEqual((byte)255, white.R);
        Assert.AreEqual((byte)255, white.G);
        Assert.AreEqual((byte)255, white.B);
        Assert.AreEqual((byte)255, white.A);
    }

    [TestMethod]
    public void Black_HasZeroRgbAndOpaqueAlpha()
    {
        Colour black = Colour.Black;
        Assert.AreEqual((byte)0, black.R);
        Assert.AreEqual((byte)0, black.G);
        Assert.AreEqual((byte)0, black.B);
        Assert.AreEqual((byte)255, black.A);
    }

    [TestMethod]
    public void Red_HasFullRedZeroOtherChannels()
    {
        Colour red = Colour.Red;
        Assert.AreEqual((byte)255, red.R);
        Assert.AreEqual((byte)0, red.G);
        Assert.AreEqual((byte)0, red.B);
        Assert.AreEqual((byte)255, red.A);
    }

    [TestMethod]
    public void Transparent_HasZeroAlpha()
    {
        Colour transparent = Colour.Transparent;
        Assert.AreEqual((byte)0, transparent.A);
    }
}
