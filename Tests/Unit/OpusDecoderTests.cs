using System;
using System.IO;
using System.Text;
using BabyBearsEngine.Platform.OpenAL;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class OpusDecoderTests
{
    // 1-second stereo 440 Hz sine wave at 44.1 kHz, encoded as Opus. Opus internally always
    // decodes at 48 kHz, so the input sample rate is upsampled by the codec. Generated once with
    // ffmpeg, committed under Tests/Unit/TestAssets/.
    private static string FixturePath => Path.Combine(AppContext.BaseDirectory, "TestAssets", "sine.opus");

    [TestMethod]
    public void Decode_FileNotFound_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => OpusDecoder.Decode("definitely-not-a-real-file.opus"));
    }

    [TestMethod]
    public void Decode_SineFixture_ReturnsStereo16At48000Hz()
    {
        DecodedAudio result = OpusDecoder.Decode(FixturePath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        // Opus always decodes at 48 kHz regardless of the source sample rate.
        Assert.AreEqual(48000, result.SampleRate);
    }

    [TestMethod]
    public void Decode_SineFixture_DurationCloseToOneSecond()
    {
        DecodedAudio result = OpusDecoder.Decode(FixturePath);

        // Opus frames cover 20 ms by default; expect close to 1 second.
        Assert.IsGreaterThan(TimeSpan.FromMilliseconds(950).Ticks, result.Duration.Ticks);
        Assert.IsLessThan(TimeSpan.FromMilliseconds(1100).Ticks, result.Duration.Ticks);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmBufferMatchesSampleCount()
    {
        DecodedAudio result = OpusDecoder.Decode(FixturePath);

        // 1 second stereo at 48000 Hz × 2 channels × 2 bytes = 192000 bytes (target).
        Assert.IsGreaterThan(185000, result.Pcm.Length);
        Assert.IsLessThan(210000, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_SineFixture_PcmContainsRealSignal()
    {
        DecodedAudio result = OpusDecoder.Decode(FixturePath);

        AudioFixtureAssertions.AssertSineWavePresent(result);
    }

    [TestMethod]
    public void ExtractOpusHeadChannels_MonoHeader_ReturnsOne()
    {
        byte[] head = BuildSyntheticOpusHead(channels: 1);

        int channels = OpusDecoder.ExtractOpusHeadChannels(head, "synthetic.opus");

        Assert.AreEqual(1, channels);
    }

    [TestMethod]
    public void ExtractOpusHeadChannels_StereoHeader_ReturnsTwo()
    {
        byte[] head = BuildSyntheticOpusHead(channels: 2);

        int channels = OpusDecoder.ExtractOpusHeadChannels(head, "synthetic.opus");

        Assert.AreEqual(2, channels);
    }

    [TestMethod]
    public void ExtractOpusHeadChannels_HeaderWithLeadingOggFraming_StillFound()
    {
        // The peek-then-scan strategy needs to skip past the Ogg page header bytes that precede the
        // payload. Construct a buffer that has some "OggS" framing junk before the OpusHead bytes
        // and verify the channel count is still extracted.
        byte[] leadingFraming = "OggS\0\0\0\0junk-page-header-bytes-here-padding"u8.ToArray();
        byte[] opusHead = BuildSyntheticOpusHead(channels: 2);

        byte[] combined = new byte[leadingFraming.Length + opusHead.Length];
        Array.Copy(leadingFraming, combined, leadingFraming.Length);
        Array.Copy(opusHead, 0, combined, leadingFraming.Length, opusHead.Length);

        int channels = OpusDecoder.ExtractOpusHeadChannels(combined, "synthetic.opus");

        Assert.AreEqual(2, channels);
    }

    [TestMethod]
    public void ExtractOpusHeadChannels_NoMagic_Throws()
    {
        byte[] head = "not an opus file at all, no magic here just text padding bytes to fill"u8.ToArray();

        Assert.ThrowsExactly<NotSupportedException>(() => OpusDecoder.ExtractOpusHeadChannels(head, "fake.opus"));
    }

    [TestMethod]
    public void ExtractOpusHeadChannels_TruncatedHeader_Throws()
    {
        // OpusHead magic present but the channel-count byte (offset +9) is past the end of the
        // buffer. The decoder must treat this as a malformed header rather than reading past the
        // buffer.
        byte[] head = "OpusHead\x01"u8.ToArray();    // 8 bytes magic + 1 byte version, no channel byte

        Assert.ThrowsExactly<NotSupportedException>(() => OpusDecoder.ExtractOpusHeadChannels(head, "truncated.opus"));
    }

    /// <summary>
    /// Builds a buffer containing a minimal valid OpusHead packet layout: "OpusHead" magic,
    /// version byte (1), channel-count byte, and enough trailing bytes that the scan finds the
    /// channel byte within the buffer.
    /// </summary>
    private static byte[] BuildSyntheticOpusHead(int channels)
    {
        byte[] magic = Encoding.ASCII.GetBytes("OpusHead");
        byte[] head = new byte[magic.Length + 11];   // magic + version + channels + a few extra bytes
        Array.Copy(magic, head, magic.Length);
        head[magic.Length] = 1;                       // version
        head[magic.Length + 1] = (byte)channels;      // channel count
        return head;
    }
}
