using System;
using System.IO;
using BabyBearsEngine.IO;
using BabyBearsEngine.Platform.OpenAL;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AssetReaderRoutingTests
{
    private const string LogicalFontPath = "Assets/Fonts/Virtual_Font.ttf";

    private IAssetReader _originalReader = new FileAssetReader();

    [TestInitialize]
    public void Setup() => _originalReader = AssetStore.Reader;

    [TestCleanup]
    public void Cleanup() => AssetStore.Reader = _originalReader;

    private static byte[] FixtureBytes(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestAssets", name));

    private static byte[] ShippedFontBytes() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "Arial.ttf"));

    private InMemoryAssetReader InstallReader(string path, byte[] content)
    {
        InMemoryAssetReader reader = new InMemoryAssetReader().Add(path, content);
        AssetStore.Reader = reader;
        return reader;
    }

    [TestMethod]
    public void AssetStore_ReadAllBytes_ReadsThroughReader()
    {
        byte[] expected = [1, 2, 3, 4];
        InstallReader("virtual/data.bin", expected);

        CollectionAssert.AreEqual(expected, AssetStore.ReadAllBytes("virtual/data.bin"));
    }

    [TestMethod]
    public void Mp3Decoder_CustomReader_DecodesPathNotOnDisk()
    {
        InMemoryAssetReader reader = InstallReader("virtual/Lose.mp3", FixtureBytes("sine.mp3"));

        DecodedAudio result = Mp3Decoder.Decode("virtual/Lose.mp3");

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(44100, result.SampleRate);
        Assert.HasCount(1, reader.OpenedPaths);
        AudioFixtureAssertions.AssertSineWavePresent(result);
    }

    [TestMethod]
    public void Mp3Decoder_CustomReaderWithoutPath_ThrowsFileNotFound()
    {
        AssetStore.Reader = new InMemoryAssetReader();

        Assert.ThrowsExactly<FileNotFoundException>(() => Mp3Decoder.Decode("virtual/missing.mp3"));
    }

    [TestMethod]
    public void OggDecoder_CustomReader_DecodesPathNotOnDisk()
    {
        InstallReader("virtual/sine.ogg", FixtureBytes("sine.ogg"));

        DecodedAudio result = OggDecoder.Decode("virtual/sine.ogg");

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }

    [TestMethod]
    public void FlacDecoder_CustomReader_DecodesPathNotOnDisk()
    {
        InstallReader("virtual/sine.flac", FixtureBytes("sine.flac"));

        DecodedAudio result = FlacDecoder.Decode("virtual/sine.flac");

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }

    [TestMethod]
    public void OpusDecoder_CustomReader_DecodesPathNotOnDisk()
    {
        InstallReader("virtual/sine.opus", FixtureBytes("sine.opus"));

        DecodedAudio result = OpusDecoder.Decode("virtual/sine.opus");

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }

    [TestMethod]
    public void TrueTypeFontFileResolver_CustomReader_ResolvesVirtualFont()
    {
        InstallReader(LogicalFontPath, ShippedFontBytes());

        Assert.AreEqual(LogicalFontPath, TrueTypeFontFileResolver.Resolve("Virtual_Font"));
    }

    [TestMethod]
    public void TrueTypeFontFileResolver_CustomReaderWithoutFont_Throws()
    {
        AssetStore.Reader = new InMemoryAssetReader();

        Assert.ThrowsExactly<FileNotFoundException>(() => TrueTypeFontFileResolver.Resolve("Arial"));
    }

    [TestMethod]
    public void FreeTypeRasteriseAtlas_CustomReader_UsesServedFont()
    {
        InMemoryAssetReader reader = InstallReader(LogicalFontPath, ShippedFontBytes());

        (byte[] pixels, int width, int height, _) = new FreeTypeFontAtlasGenerator().RasteriseAtlas(new FontDefinition("Virtual_Font", 32f));

        Assert.IsGreaterThan(0, width);
        Assert.AreEqual(width * height, pixels.Length);
        Assert.Contains(LogicalFontPath, reader.OpenedPaths);
    }

    [TestMethod]
    public void SdfRasteriseAtlas_CustomReader_UsesServedFont()
    {
        InMemoryAssetReader reader = InstallReader(LogicalFontPath, ShippedFontBytes());

        (byte[] pixels, int width, int height, _) = new SdfFontAtlasGenerator().RasteriseAtlas(new FontDefinition("Virtual_Font", 32f));

        Assert.IsGreaterThan(0, width);
        Assert.AreEqual(width * height, pixels.Length);
        Assert.Contains(LogicalFontPath, reader.OpenedPaths);
    }
}
