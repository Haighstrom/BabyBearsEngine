using System;
using System.IO;
using BabyBearsEngine.IO;
using BabyBearsEngine.Platform.ImageLoading;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ImageLoaderTests
{
    private sealed class FakeAssetReader(byte[] content) : IAssetReader
    {
        public List<string> OpenedPaths { get; } = new();

        public bool Exists(string path) => true;

        public Stream OpenRead(string path)
        {
            OpenedPaths.Add(path);
            return new MemoryStream(content);
        }
    }

    // 2x2 PNG authored outside the engine: red, green on the top row; blue, half-transparent white below.
    private const string ReferencePngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAUSURBVBhXY/jPwPAfDBkY/oNAAwBJSQl4ztdj9wAAAABJRU5ErkJggg==";

    private static readonly byte[] s_referencePixels =
    [
        255, 0, 0, 255,
        0, 255, 0, 255,
        0, 0, 255, 255,
        255, 255, 255, 128,
    ];

    private IAssetReader _originalReader = new FileAssetReader();
    private string _tempDir = "";

    [TestInitialize]
    public void Setup()
    {
        _originalReader = AssetStore.Reader;
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        AssetStore.Reader = _originalReader;

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
    public void LoadAsRgba8_CustomReader_LoadsThroughReader()
    {
        FakeAssetReader reader = new(Convert.FromBase64String(ReferencePngBase64));
        AssetStore.Reader = reader;

        Rgba8ImageData imageData = ImageLoader.LoadAsRgba8("virtual/scrambled.dat");

        Assert.HasCount(1, reader.OpenedPaths);
        Assert.AreEqual("virtual/scrambled.dat", reader.OpenedPaths[0]);
        CollectionAssert.AreEqual(s_referencePixels, imageData.Data);
    }

    [TestMethod]
    public void AssetStore_SetNullReader_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => AssetStore.Reader = null!);
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
