using System;
using System.IO;
using BabyBearsEngine.Platform.ImageLoading;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ImageLoaderTests
{
    // 2x2 PNG authored outside the engine: red, green on the top row; blue, half-transparent white below.
    private const string ReferencePngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAUSURBVBhXY/jPwPAfDBkY/oNAAwBJSQl4ztdj9wAAAABJRU5ErkJggg==";

    private static readonly byte[] s_referencePixels =
    [
        255, 0, 0, 255,
        0, 255, 0, 255,
        0, 0, 255, 255,
        255, 255, 255, 128,
    ];

    private string _tempDir = "";

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private string TempFile(string name = "test.png") => Path.Combine(_tempDir, name);

    [TestMethod]
    public void LoadAsRgba8_ReferencePng_ReturnsDimensions()
    {
        File.WriteAllBytes(TempFile(), Convert.FromBase64String(ReferencePngBase64));

        Rgba8ImageData imageData = ImageLoader.LoadAsRgba8(TempFile());

        Assert.AreEqual(2, imageData.Width);
        Assert.AreEqual(2, imageData.Height);
    }

    [TestMethod]
    public void LoadAsRgba8_ReferencePng_ReturnsStraightAlphaRgbaTopRowFirst()
    {
        File.WriteAllBytes(TempFile(), Convert.FromBase64String(ReferencePngBase64));

        Rgba8ImageData imageData = ImageLoader.LoadAsRgba8(TempFile());

        CollectionAssert.AreEqual(s_referencePixels, imageData.Data);
    }

    [TestMethod]
    public void LoadAsRgba8_MissingFile_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => ImageLoader.LoadAsRgba8(TempFile("missing.png")));
    }

    [TestMethod]
    public void SaveAsPng_ThenLoadAsRgba8_RoundTrips()
    {
        ImageSaver.SaveAsPng(new Rgba8ImageData(Width: 2, Height: 2, s_referencePixels), TempFile());

        Rgba8ImageData imageData = ImageLoader.LoadAsRgba8(TempFile());

        Assert.AreEqual(2, imageData.Width);
        Assert.AreEqual(2, imageData.Height);
        CollectionAssert.AreEqual(s_referencePixels, imageData.Data);
    }

    [TestMethod]
    public void SaveAsPng_NonSquareImage_PreservesWidthAndHeight()
    {
        byte[] pixels = new byte[3 * 1 * 4];

        ImageSaver.SaveAsPng(new Rgba8ImageData(Width: 3, Height: 1, pixels), TempFile());

        Rgba8ImageData imageData = ImageLoader.LoadAsRgba8(TempFile());

        Assert.AreEqual(3, imageData.Width);
        Assert.AreEqual(1, imageData.Height);
    }
}
