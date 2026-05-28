using System;
using System.Linq;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FreeTypeFontAtlasGeneratorTests
{
    // A real TrueType file the engine ships in Assets/Fonts/.
    private const string TestFontName = "Arial";

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
        FreeTypeFontAtlasGenerator generator = new();
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
        FreeTypeFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, _, _, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        Assert.IsGreaterThan(0, metrics.WidestChar);
        Assert.IsGreaterThan(0, metrics.HighestChar);
    }

    [TestMethod]
    public void RasteriseAtlas_EveryRequestedCharacterHasMetrics()
    {
        FreeTypeFontAtlasGenerator generator = new();
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
        FreeTypeFontAtlasGenerator generator = new();
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
    public void RasteriseAtlas_AdvanceIsPositiveForEveryCharacter()
    {
        FreeTypeFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, _, _, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        foreach (char c in fontDef.CharactersToLoad)
        {
            Assert.IsGreaterThan(0, metrics.GetCharAdvance(c), $"Non-positive advance for '{c}' (U+{(int)c:X4}).");
        }
    }

    [TestMethod]
    public void RasteriseAtlas_SpaceHasAdvanceButNoRenderQuad()
    {
        FreeTypeFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, _, _, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        // Space has no coverage: it contributes spacing (a positive advance) but draws no quad, so
        // its render box collapses to zero width.
        Assert.IsGreaterThan(0, metrics.GetCharAdvance(' '));
        Assert.AreEqual(0, metrics.GetCharPosition(' ').Size.X);
    }

    [TestMethod]
    public void RasteriseAtlas_RenderBoxMatchesNormalisedUvExtent()
    {
        FreeTypeFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (_, int width, int height, FontAtlasMetrics metrics) = generator.RasteriseAtlas(fontDef);

        // Coverage has no glow margin: the render quad maps 1:1 to the tight glyph bitmap, so the
        // logical render width must equal the normalised UV span scaled back to atlas pixels.
        Box2i renderBox = metrics.GetCharPosition('c');
        Box2 uv = metrics.GetCharPositionNormalised('c');

        int uvWidthPixels = (int)MathF.Round((uv.Max.X - uv.Min.X) * width);
        int uvHeightPixels = (int)MathF.Round((uv.Max.Y - uv.Min.Y) * height);

        Assert.AreEqual(uvWidthPixels, renderBox.Size.X);
        Assert.AreEqual(uvHeightPixels, renderBox.Size.Y);
    }

    [TestMethod]
    public void RasteriseAtlas_ContainsHighCoverageInkedTexels()
    {
        FreeTypeFontAtlasGenerator generator = new();
        FontDefinition fontDef = new(TestFontName, 48f);

        (byte[] pixels, _, _, _) = generator.RasteriseAtlas(fontDef);

        // Coverage stores alpha 0..255; solid glyph interiors must reach near-full coverage,
        // confirming glyphs were actually rasterised (not an empty atlas).
        int maxValue = pixels.Max(pixel => (int)pixel);
        Assert.IsGreaterThan(200, maxValue);
    }

    [TestMethod]
    public void RasteriseAtlas_SourceResolutionScalesWithFontSize()
    {
        FreeTypeFontAtlasGenerator generator = new();

        (_, int smallWidth, int smallHeight, FontAtlasMetrics smallMetrics) = generator.RasteriseAtlas(new FontDefinition(TestFontName, 24f));
        (_, int largeWidth, int largeHeight, FontAtlasMetrics largeMetrics) = generator.RasteriseAtlas(new FontDefinition(TestFontName, 48f));

        // Coverage is authored at the requested pixel size, so a larger font rasterises to a larger
        // source atlas...
        Assert.IsGreaterThan(smallWidth, largeWidth);
        Assert.IsGreaterThan(smallHeight, largeHeight);

        // ...and the logical line height scales with it too.
        Assert.IsGreaterThan(smallMetrics.HighestChar, largeMetrics.HighestChar);
    }
}
