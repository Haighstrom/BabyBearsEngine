using System;
using System.IO;
using BabyBearsEngine.Platform.OpenAL;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class OggDecoderTests
{
    // The fixture is a 1-second stereo 440 Hz sine wave at 44.1 kHz, encoded as Ogg Vorbis at
    // default quality. Generated once with ffmpeg and committed under Tests/Unit/TestAssets/.
    private static string FixturePath => Path.Combine(AppContext.BaseDirectory, "TestAssets", "sine.ogg");

    [TestMethod]
    public void Decode_FileNotFound_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => OggDecoder.Decode("definitely-not-a-real-file.ogg"));
    }

    [TestMethod]
    public void Decode_SineFixture_ReturnsStereo16At44100Hz()
    {
        DecodedAudio result = OggDecoder.Decode(FixturePath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(44100, result.SampleRate);
    }

    [TestMethod]
    public void Decode_SineFixture_DurationCloseToOneSecond()
    {
        DecodedAudio result = OggDecoder.Decode(FixturePath);

        // Vorbis frames are typically 256/512/1024/2048 samples, so the decoded duration can land
        // a few ms off the encoder's request — accept anything inside [950, 1100] ms.
        Assert.IsGreaterThan(TimeSpan.FromMilliseconds(950).Ticks, result.Duration.Ticks);
        Assert.IsLessThan(TimeSpan.FromMilliseconds(1100).Ticks, result.Duration.Ticks);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmBufferMatchesSampleCount()
    {
        DecodedAudio result = OggDecoder.Decode(FixturePath);

        // 1 second stereo at 44.1 kHz = 44100 frames × 2 channels × 2 bytes = 176400 bytes (target).
        // Codec frame boundaries can add a few hundred samples either side.
        Assert.IsGreaterThan(170000, result.Pcm.Length);
        Assert.IsLessThan(190000, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmContainsRealSignal()
    {
        DecodedAudio result = OggDecoder.Decode(FixturePath);

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }
}
