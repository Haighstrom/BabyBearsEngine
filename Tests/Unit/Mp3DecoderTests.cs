using System;
using System.IO;
using BabyBearsEngine.Platform.OpenAL;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Mp3DecoderTests
{
    // 1-second stereo 440 Hz sine wave at 44.1 kHz, encoded as MP3 at 128 kbps CBR.
    // Generated once with ffmpeg and committed under Tests/Unit/TestAssets/.
    private static string FixturePath => Path.Combine(AppContext.BaseDirectory, "TestAssets", "sine.mp3");

    [TestMethod]
    public void Decode_FileNotFound_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => Mp3Decoder.Decode("definitely-not-a-real-file.mp3"));
    }

    [TestMethod]
    public void Decode_SineFixture_ReturnsStereo16At44100Hz()
    {
        DecodedAudio result = Mp3Decoder.Decode(FixturePath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(44100, result.SampleRate);
    }

    [TestMethod]
    public void Decode_SineFixture_DurationCloseToOneSecond()
    {
        DecodedAudio result = Mp3Decoder.Decode(FixturePath);

        // MP3 frames cover 1152 samples each, plus encoder/decoder priming silence, so the decoded
        // duration is typically ~50ms longer than the encoder request. Accept [950, 1150] ms.
        Assert.IsGreaterThan(TimeSpan.FromMilliseconds(950).Ticks, result.Duration.Ticks);
        Assert.IsLessThan(TimeSpan.FromMilliseconds(1150).Ticks, result.Duration.Ticks);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmBufferMatchesSampleCount()
    {
        DecodedAudio result = Mp3Decoder.Decode(FixturePath);

        // 1 second stereo at 44.1 kHz = 44100 × 2 × 2 = 176400 bytes (target). MP3's frame-aligned
        // output plus LAME priming is usually a bit larger.
        Assert.IsGreaterThan(170000, result.Pcm.Length);
        Assert.IsLessThan(200000, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmContainsRealSignal()
    {
        DecodedAudio result = Mp3Decoder.Decode(FixturePath);

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }
}
