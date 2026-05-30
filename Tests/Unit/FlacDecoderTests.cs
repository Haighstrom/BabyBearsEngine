using System;
using System.IO;
using BabyBearsEngine.Platform.OpenAL;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FlacDecoderTests
{
    // 1-second stereo 440 Hz sine wave at 44.1 kHz, encoded as FLAC at default quality.
    // Generated once with ffmpeg and committed under Tests/Unit/TestAssets/.
    private static string FixturePath => Path.Combine(AppContext.BaseDirectory, "TestAssets", "sine.flac");

    [TestMethod]
    public void Decode_FileNotFound_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => FlacDecoder.Decode("definitely-not-a-real-file.flac"));
    }

    [TestMethod]
    public void Decode_SineFixture_ReturnsStereo16At44100Hz()
    {
        DecodedAudio result = FlacDecoder.Decode(FixturePath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(44100, result.SampleRate);
    }

    [TestMethod]
    public void Decode_SineFixture_DurationCloseToOneSecond()
    {
        DecodedAudio result = FlacDecoder.Decode(FixturePath);

        // FLAC is lossless and frame-aligned, so the duration should land essentially on the nose.
        Assert.IsGreaterThan(TimeSpan.FromMilliseconds(990).Ticks, result.Duration.Ticks);
        Assert.IsLessThan(TimeSpan.FromMilliseconds(1010).Ticks, result.Duration.Ticks);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmBufferMatchesSampleCount()
    {
        DecodedAudio result = FlacDecoder.Decode(FixturePath);

        // 1 second stereo at 44.1 kHz × 2 channels × 2 bytes = 176400 bytes exactly. FLAC is
        // lossless so we should land within a few samples of that.
        Assert.IsGreaterThan(175000, result.Pcm.Length);
        Assert.IsLessThan(177000, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmContainsRealSignal()
    {
        DecodedAudio result = FlacDecoder.Decode(FixturePath);

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }
}
