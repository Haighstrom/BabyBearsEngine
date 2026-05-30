using System;
using BabyBearsEngine.Platform.OpenAL;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PcmConvertTests
{
    [TestMethod]
    public void FloatInterleavedToPcm16_Zero_ProducesZeroBytes()
    {
        float[] samples = new float[4];   // all zeros

        byte[] pcm = PcmConvert.FloatInterleavedToPcm16(samples, samples.Length);

        Assert.AreEqual(8, pcm.Length);
        for (int i = 0; i < pcm.Length; i++)
        {
            Assert.AreEqual(0, pcm[i]);
        }
    }

    [TestMethod]
    public void FloatInterleavedToPcm16_PositiveOne_ProducesShortMax()
    {
        float[] samples = [1f];

        byte[] pcm = PcmConvert.FloatInterleavedToPcm16(samples, 1);

        // short.MaxValue = 0x7FFF → little-endian bytes 0xFF, 0x7F
        Assert.AreEqual(0xFF, pcm[0]);
        Assert.AreEqual(0x7F, pcm[1]);
    }

    [TestMethod]
    public void FloatInterleavedToPcm16_NegativeOne_ProducesNegativeShortMax()
    {
        float[] samples = [-1f];

        byte[] pcm = PcmConvert.FloatInterleavedToPcm16(samples, 1);

        // -1 * short.MaxValue = -32767 → little-endian bytes 0x01, 0x80
        Assert.AreEqual(0x01, pcm[0]);
        Assert.AreEqual(0x80, pcm[1]);
    }

    [TestMethod]
    public void FloatInterleavedToPcm16_ValueAbove1_ClampsToMax()
    {
        float[] samples = [2.5f, -2.5f];

        byte[] pcm = PcmConvert.FloatInterleavedToPcm16(samples, 2);

        Assert.AreEqual(0xFF, pcm[0]);
        Assert.AreEqual(0x7F, pcm[1]);
        Assert.AreEqual(0x01, pcm[2]);
        Assert.AreEqual(0x80, pcm[3]);
    }

    [TestMethod]
    public void FloatInterleavedToPcm16_RespectsSampleCount()
    {
        // The conversion should only process the first sampleCount items, ignoring the tail of the
        // buffer — decoders typically work into a pre-allocated buffer and report how much was
        // actually filled.
        float[] samples = new float[10];
        samples[0] = 1f;
        samples[1] = 1f;
        // samples[2..] = 0

        byte[] pcm = PcmConvert.FloatInterleavedToPcm16(samples, 2);

        Assert.AreEqual(4, pcm.Length);
    }

    [TestMethod]
    public void ShortInterleavedToPcm16_LittleEndianRoundTrip()
    {
        short[] samples = [0x1234, unchecked((short)0xABCD), 0x0000, 0x7FFF];

        byte[] pcm = PcmConvert.ShortInterleavedToPcm16(samples, samples.Length);

        Assert.AreEqual(8, pcm.Length);
        Assert.AreEqual(0x34, pcm[0]);
        Assert.AreEqual(0x12, pcm[1]);
        Assert.AreEqual(0xCD, pcm[2]);
        Assert.AreEqual(0xAB, pcm[3]);
        Assert.AreEqual(0x00, pcm[4]);
        Assert.AreEqual(0x00, pcm[5]);
        Assert.AreEqual(0xFF, pcm[6]);
        Assert.AreEqual(0x7F, pcm[7]);
    }

    [TestMethod]
    public void GetSoundFormat16_Mono_ReturnsMono16()
    {
        Assert.AreEqual(ALFormat.Mono16, PcmConvert.GetSoundFormat16(1, "test.wav"));
    }

    [TestMethod]
    public void GetSoundFormat16_Stereo_ReturnsStereo16()
    {
        Assert.AreEqual(ALFormat.Stereo16, PcmConvert.GetSoundFormat16(2, "test.wav"));
    }

    [TestMethod]
    public void GetSoundFormat16_UnsupportedChannelCount_Throws()
    {
        Assert.ThrowsExactly<NotSupportedException>(() => PcmConvert.GetSoundFormat16(0, "test.wav"));
        Assert.ThrowsExactly<NotSupportedException>(() => PcmConvert.GetSoundFormat16(3, "test.wav"));
        Assert.ThrowsExactly<NotSupportedException>(() => PcmConvert.GetSoundFormat16(6, "test.wav"));
    }
}
