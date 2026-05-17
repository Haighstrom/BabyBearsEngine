using System;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ColourHexTests
{
    // ToHex

    [TestMethod]
    public void ToHex_FullyOpaque_IncludesAlphaFF()
    {
        Colour colour = new(255, 0, 0);

        Assert.AreEqual("#FF0000FF", colour.ToHex());
    }

    [TestMethod]
    public void ToHex_WithAlpha_IncludesAlphaInOutput()
    {
        Colour colour = new(0, 128, 255, 64);

        Assert.AreEqual("#0080FF40", colour.ToHex());
    }

    [TestMethod]
    public void ToHex_Black_ReturnsAllZeroesWithFF()
    {
        Assert.AreEqual("#000000FF", Colour.Black.ToHex());
    }

    [TestMethod]
    public void ToHex_White_ReturnsAllFF()
    {
        Assert.AreEqual("#FFFFFFFF", Colour.White.ToHex());
    }

    // Colour(string hex) — #RRGGBB

    [TestMethod]
    public void HexConstructor_SixDigit_ParsesRGBAndDefaultsAlphaTo255()
    {
        Colour colour = new("#FF4400");

        Assert.AreEqual(255, colour.R);
        Assert.AreEqual(68, colour.G);
        Assert.AreEqual(0, colour.B);
        Assert.AreEqual(255, colour.A);
    }

    [TestMethod]
    public void HexConstructor_SixDigit_NoHash_ParsesRGB()
    {
        Colour colour = new("FF4400");

        Assert.AreEqual(255, colour.R);
        Assert.AreEqual(68, colour.G);
        Assert.AreEqual(0, colour.B);
    }

    // Colour(string hex) — #RRGGBBAA

    [TestMethod]
    public void HexConstructor_EightDigit_ParsesRGBAIncludingAlpha()
    {
        Colour colour = new("#0080FF40");

        Assert.AreEqual(0, colour.R);
        Assert.AreEqual(128, colour.G);
        Assert.AreEqual(255, colour.B);
        Assert.AreEqual(64, colour.A);
    }

    [TestMethod]
    public void HexConstructor_EightDigit_NoHash_ParsesRGBA()
    {
        Colour colour = new("0080FF40");

        Assert.AreEqual(0, colour.R);
        Assert.AreEqual(128, colour.G);
        Assert.AreEqual(255, colour.B);
        Assert.AreEqual(64, colour.A);
    }

    // Round-trip

    [TestMethod]
    public void ToHex_ThenHexConstructor_RoundTrips()
    {
        Colour original = new(12, 34, 56, 78);
        Colour roundTripped = new(original.ToHex());

        Assert.AreEqual(original, roundTripped);
    }

    // Invalid input

    [TestMethod]
    public void HexConstructor_InvalidLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => _ = new Colour("#FFF"));
    }

    [TestMethod]
    public void HexConstructor_InvalidCharacters_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => _ = new Colour("#GGGGGG"));
    }
}
