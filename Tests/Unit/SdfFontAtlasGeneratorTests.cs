using System;
using System.Linq;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SdfFontAtlasGeneratorTests
{
    // A real TrueType file the engine ships in Assets/Fonts/.
    private const string TestFontName = "Arial";

    // Mirrors SdfFontAtlasGenerator.OnEdgeValue: the SDF byte value at the glyph outline.
    // Texels inside the glyph store values above this.
    private const int OnEdgeValue = 128;

    private string _originalDirectory = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        // RasteriseAtlas resolves the font via a relative path, so anchor the working directory
        // to the test output where the engine's fonts are copied.
        _originalDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = AppContext.BaseDirectory;
    }

    [TestCleanup]
    public void Cleanup() => Environment.CurrentDirectory = _originalDirectory;

    [TestMethod]
    public void RasteriseAtlas_ProducesSingleChannelBufferMatchingDimensions()
    {
        SdfFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (byte[] pixels, int width, int height, _) = generator.RasteriseAtlas(fontDef);

        Assert.IsGreaterThan(0, width);
        Assert.IsGreaterThan(0, height);

        // One byte per texel (R8): the buffer length must equal width * height.
        Assert.AreEqual(width * height, pixels.Length);
    }

    [TestMethod]
    public void RasteriseAtlas_MetricsArePositive()
    {
        SdfFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, _, _, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        Assert.IsGreaterThan(0, metrics.WidestChar);
        Assert.IsGreaterThan(0, metrics.HighestChar);
    }

    [TestMethod]
    public void RasteriseAtlas_EveryRequestedCharacterHasMetrics()
    {
        SdfFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, _, _, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        foreach (char c in fontDef.CharactersToLoad)
        {
            Assert.IsTrue(metrics.CharPositions.ContainsKey(c), $"Missing logical position for '{c}' (U+{(int)c:X4}).");
            Assert.IsTrue(metrics.CharPositionsNormalised.ContainsKey(c), $"Missing normalised position for '{c}' (U+{(int)c:X4}).");
        }
    }

    [TestMethod]
    public void RasteriseAtlas_NormalisedPositionsWithinUnitRange()
    {
        SdfFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, _, _, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        foreach (Box2 box in metrics.CharPositionsNormalised.Values)
        {
            Assert.IsGreaterThanOrEqualTo(0f, box.Min.X);
            Assert.IsGreaterThanOrEqualTo(0f, box.Min.Y);
            Assert.IsLessThanOrEqualTo(1f, box.Max.X);
            Assert.IsLessThanOrEqualTo(1f, box.Max.Y);
        }
    }

    [TestMethod]
    public void RasteriseAtlas_ContainsInsideGlyphDistances()
    {
        SdfFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (byte[] pixels, _, _, _) = generator.RasteriseAtlas(fontDef);

        // If any glyph was actually rasterised, the atlas must contain texels inside the
        // outline, whose distance value exceeds the on-edge value.
        int maxValue = pixels.Max(p => (int)p);
        Assert.IsGreaterThan(OnEdgeValue, maxValue);
    }

    [TestMethod]
    public void RasteriseAtlas_SourceResolutionScalesWithFontSize()
    {
        SdfFontAtlasGenerator generator = new();

        (_, int smallWidth, int smallHeight, FontAtlasMetrics smallMetrics) = generator.RasteriseAtlas(new FontDefinition(TestFontName, 24f));
        (_, int largeWidth, int largeHeight, FontAtlasMetrics largeMetrics) = generator.RasteriseAtlas(new FontDefinition(TestFontName, 48f));

        // The distance field is authored at (about) the requested display size so on-screen
        // sampling stays near 1:1 and thin strokes don't fragment under minification. A larger
        // font size therefore rasterises to a larger source atlas...
        Assert.IsGreaterThan(smallWidth, largeWidth);
        Assert.IsGreaterThan(smallHeight, largeHeight);

        // ...and the logical layout metrics scale with it too.
        Assert.IsGreaterThan(smallMetrics.HighestChar, largeMetrics.HighestChar);
    }
}
