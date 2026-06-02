using System;
using System.IO;
using BabyBearsEngine.Platform.OpenAL;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WavDecoderTests
{
    private string _tempPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"bbe-wav-test-{Guid.NewGuid():N}.wav");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_tempPath))
        {
            File.Delete(_tempPath);
        }
    }

    [TestMethod]
    public void Decode_Mono16_ReturnsCorrectFormatAndSampleRate()
    {
        WriteWav(channels: 1, bitsPerSample: 16, sampleRate: 44100, samples: 100);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(ALFormat.Mono16, result.Format);
        Assert.AreEqual(44100, result.SampleRate);
        Assert.AreEqual(200, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_Stereo16_ReturnsCorrectFormat()
    {
        WriteWav(channels: 2, bitsPerSample: 16, sampleRate: 48000, samples: 50);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(48000, result.SampleRate);
        Assert.AreEqual(200, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_Mono8_ReturnsCorrectFormat()
    {
        WriteWav(channels: 1, bitsPerSample: 8, sampleRate: 22050, samples: 64);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(ALFormat.Mono8, result.Format);
        Assert.AreEqual(64, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_ComputesDurationFromSampleCount()
    {
        // 1000 sample frames @ 1000 Hz = 1 second
        WriteWav(channels: 1, bitsPerSample: 16, sampleRate: 1000, samples: 1000);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(TimeSpan.FromSeconds(1), result.Duration);
    }

    [TestMethod]
    public void Decode_FileNotFound_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => WavDecoder.Decode("definitely-not-a-real-file.wav"));
    }

    [TestMethod]
    public void Decode_NonRiffFile_Throws()
    {
        File.WriteAllBytes(_tempPath, "JUNK"u8.ToArray());
        Assert.ThrowsExactly<NotSupportedException>(() => WavDecoder.Decode(_tempPath));
    }

    [TestMethod]
    public void Decode_UnsupportedChannelCount_Throws()
    {
        WriteWav(channels: 5, bitsPerSample: 16, sampleRate: 44100, samples: 10);
        Assert.ThrowsExactly<NotSupportedException>(() => WavDecoder.Decode(_tempPath));
    }

    [TestMethod]
    public void Decode_NonPcmFormat_Throws()
    {
        // Write a WAV with audio format = 3 (IEEE float) which is not supported.
        WriteRawWav(audioFormat: 3, channels: 1, bitsPerSample: 32, sampleRate: 44100, dataBytes: new byte[8]);
        Assert.ThrowsExactly<NotSupportedException>(() => WavDecoder.Decode(_tempPath));
    }

    [TestMethod]
    public void Decode_Mono24_DownConvertsTo16Bit()
    {
        // 24-bit PCM is common from modern editors. The decoder accepts it and downconverts to
        // 16-bit by dropping the low byte of each 3-byte sample.
        WriteWav(channels: 1, bitsPerSample: 24, sampleRate: 44100, samples: 10);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(ALFormat.Mono16, result.Format);
        Assert.AreEqual(44100, result.SampleRate);
        Assert.AreEqual(20, result.Pcm.Length);   // 10 samples × 2 bytes after downconversion
    }

    [TestMethod]
    public void Decode_Stereo24_DownConvertsTo16Bit()
    {
        WriteWav(channels: 2, bitsPerSample: 24, sampleRate: 48000, samples: 50);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(200, result.Pcm.Length);  // 50 frames × 2 channels × 2 bytes
    }

    [TestMethod]
    public void Decode_24BitConversion_PreservesUpperBytes()
    {
        // Spot-check the downconversion: a 24-bit sample [0x11, 0x22, 0x33] should become the
        // 16-bit sample [0x22, 0x33] (low byte dropped, mid + high byte preserved as little-endian).
        byte[] sample = [0x11, 0x22, 0x33];
        WriteRawWav(audioFormat: 1, channels: 1, bitsPerSample: 24, sampleRate: 44100, dataBytes: sample);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(2, result.Pcm.Length);
        Assert.AreEqual(0x22, result.Pcm[0]);
        Assert.AreEqual(0x33, result.Pcm[1]);
    }

    [TestMethod]
    public void Decode_WaveFormatExtensiblePcm_Accepted()
    {
        // Real-world WAVs from modern encoders (mixkit, DAWs) commonly use WAVE_FORMAT_EXTENSIBLE
        // with the PCM sub-format GUID rather than the basic WAVE_FORMAT_PCM code. The decoder must
        // recognise this as still being linear PCM.
        WriteExtensibleWav(channels: 2, bitsPerSample: 16, sampleRate: 48000, samples: 100, subFormatPcm: true);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(ALFormat.Stereo16, result.Format);
        Assert.AreEqual(48000, result.SampleRate);
        Assert.AreEqual(400, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_WaveFormatExtensibleNonPcm_Throws()
    {
        // EXTENSIBLE with IEEE float sub-format — still not supported.
        WriteExtensibleWav(channels: 2, bitsPerSample: 16, sampleRate: 48000, samples: 100, subFormatPcm: false);
        Assert.ThrowsExactly<NotSupportedException>(() => WavDecoder.Decode(_tempPath));
    }

    [TestMethod]
    public void Decode_HandlesExtraChunksBeforeData()
    {
        // Insert a LIST/INFO chunk between fmt and data — common in real-world WAVs.
        using FileStream stream = File.Create(_tempPath);
        using BinaryWriter writer = new(stream);

        int dataSize = 20;
        int listSize = 8;  // four bytes of "LIST" identifier + four bytes payload
        int fmtSize = 16;
        int totalRiffSize = 4 + (8 + fmtSize) + (8 + listSize) + (8 + dataSize);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(totalRiffSize);
        writer.Write("WAVE"u8.ToArray());

        writer.Write("fmt "u8.ToArray());
        writer.Write(fmtSize);
        writer.Write((ushort)1);          // PCM
        writer.Write((ushort)1);          // mono
        writer.Write(44100);              // sample rate
        writer.Write(88200);              // byte rate
        writer.Write((ushort)2);          // block align
        writer.Write((ushort)16);         // bits per sample

        writer.Write("LIST"u8.ToArray());
        writer.Write(listSize);
        writer.Write(new byte[listSize]);

        writer.Write("data"u8.ToArray());
        writer.Write(dataSize);
        writer.Write(new byte[dataSize]);

        writer.Flush();
        stream.Close();

        DecodedAudio result = WavDecoder.Decode(_tempPath);
        Assert.AreEqual(20, result.Pcm.Length);
        Assert.AreEqual(ALFormat.Mono16, result.Format);
    }

    [TestMethod]
    public void Decode_ChunkIdWithHighBitByte_DoesNotThrowDecoderFallback()
    {
        // A rogue/corrupt WAV with a high-bit byte in an unknown chunk ID would explode
        // BinaryReader.ReadChars under default UTF-8. The decoder must read chunk IDs as raw
        // ASCII bytes and tolerate any byte sequence in unrecognised chunks.
        using (FileStream stream = File.Create(_tempPath))
        using (BinaryWriter writer = new(stream))
        {
            int dataSize = 4;
            int fmtSize = 16;
            int bogusSize = 4;
            int totalRiffSize = 4 + (8 + fmtSize) + (8 + bogusSize) + (8 + dataSize);

            writer.Write("RIFF"u8.ToArray());
            writer.Write(totalRiffSize);
            writer.Write("WAVE"u8.ToArray());

            writer.Write("fmt "u8.ToArray());
            writer.Write(fmtSize);
            writer.Write((ushort)1); writer.Write((ushort)1); writer.Write(44100); writer.Write(88200); writer.Write((ushort)2); writer.Write((ushort)16);

            // Unrecognised chunk with a high-bit byte in its ID — must not throw.
            writer.Write(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC });
            writer.Write(bogusSize);
            writer.Write(new byte[bogusSize]);

            writer.Write("data"u8.ToArray());
            writer.Write(dataSize);
            writer.Write(new byte[dataSize]);
        }

        DecodedAudio result = WavDecoder.Decode(_tempPath);
        Assert.AreEqual(4, result.Pcm.Length);
    }

    [TestMethod]
    public void Decode_NegativeChunkSize_Throws()
    {
        // A malicious chunkSize of -1 would otherwise drive stream.Position backwards.
        using (FileStream stream = File.Create(_tempPath))
        using (BinaryWriter writer = new(stream))
        {
            writer.Write("RIFF"u8.ToArray());
            writer.Write(36);
            writer.Write("WAVE"u8.ToArray());
            writer.Write("xxxx"u8.ToArray());
            writer.Write(-1);  // negative chunk size
            writer.Write(new byte[20]);
        }

        Assert.ThrowsExactly<NotSupportedException>(() => WavDecoder.Decode(_tempPath));
    }

    [TestMethod]
    public void Decode_ChunkSizeExceedsRemainingStream_Throws()
    {
        // chunkSize larger than the file would force ReadBytes to allocate a huge buffer and
        // then short-read. The decoder should reject these up front rather than allocate first.
        using (FileStream stream = File.Create(_tempPath))
        using (BinaryWriter writer = new(stream))
        {
            writer.Write("RIFF"u8.ToArray());
            writer.Write(36);
            writer.Write("WAVE"u8.ToArray());
            writer.Write("xxxx"u8.ToArray());
            writer.Write(int.MaxValue);  // way bigger than the file
            writer.Write(new byte[8]);
        }

        Assert.ThrowsExactly<NotSupportedException>(() => WavDecoder.Decode(_tempPath));
    }

    [TestMethod]
    public void Decode_24BitConversion_DoesNotIntroduceSignBiasOnNegativeSamples()
    {
        // 24-bit sample 0x800001 (negative, just above the floor): low=0x01, mid=0x00, high=0x80.
        // Dropping the low byte gives 16-bit 0x8000 — exact value, no bias. The previous comment
        // claimed "truncation toward zero" but the implementation just drops the low byte
        // (arithmetic shift, rounds toward negative infinity). The behaviour itself is fine —
        // this test pins down the result for the negative-sample boundary case so we don't
        // accidentally change it.
        byte[] sample = [0x01, 0x00, 0x80];
        WriteRawWav(audioFormat: 1, channels: 1, bitsPerSample: 24, sampleRate: 44100, dataBytes: sample);

        DecodedAudio result = WavDecoder.Decode(_tempPath);

        Assert.AreEqual(2, result.Pcm.Length);
        Assert.AreEqual(0x00, result.Pcm[0]);
        Assert.AreEqual(0x80, result.Pcm[1]);
    }

    private void WriteWav(int channels, int bitsPerSample, int sampleRate, int samples)
    {
        int bytesPerSample = bitsPerSample / 8;
        int dataBytes = samples * channels * bytesPerSample;
        byte[] data = new byte[dataBytes];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i % 256);
        }
        WriteRawWav(audioFormat: 1, channels: channels, bitsPerSample: bitsPerSample, sampleRate: sampleRate, dataBytes: data);
    }

    private void WriteExtensibleWav(int channels, int bitsPerSample, int sampleRate, int samples, bool subFormatPcm)
    {
        int bytesPerSample = bitsPerSample / 8;
        int dataBytes = samples * channels * bytesPerSample;
        byte[] data = new byte[dataBytes];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i % 256);
        }

        using FileStream stream = File.Create(_tempPath);
        using BinaryWriter writer = new(stream);

        int byteRate = sampleRate * channels * bytesPerSample;
        int blockAlign = channels * bytesPerSample;
        const int fmtSize = 40;   // 16 basic + 2 cbSize + 22 extension
        int totalRiffSize = 4 + (8 + fmtSize) + (8 + dataBytes);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(totalRiffSize);
        writer.Write("WAVE"u8.ToArray());

        writer.Write("fmt "u8.ToArray());
        writer.Write(fmtSize);
        writer.Write((ushort)0xFFFE);              // WAVE_FORMAT_EXTENSIBLE
        writer.Write((ushort)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((ushort)blockAlign);
        writer.Write((ushort)bitsPerSample);

        // Extension
        writer.Write((ushort)22);                  // cbSize
        writer.Write((ushort)bitsPerSample);       // wValidBitsPerSample
        writer.Write((uint)0x3);                   // dwChannelMask (front L + R)
        // SubFormat GUID: first 4 bytes are 1 for PCM, 3 for IEEE float; remaining 12 are the fixed
        // KSDATAFORMAT suffix {00000000-0000-0010-8000-00aa00389b71}.
        writer.Write(subFormatPcm ? (uint)1 : (uint)3);
        writer.Write((ushort)0x0000);
        writer.Write((ushort)0x0010);
        writer.Write(new byte[] { 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71 });

        writer.Write("data"u8.ToArray());
        writer.Write(dataBytes);
        writer.Write(data);
    }

    private void WriteRawWav(int audioFormat, int channels, int bitsPerSample, int sampleRate, byte[] dataBytes)
    {
        using FileStream stream = File.Create(_tempPath);
        using BinaryWriter writer = new(stream);

        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        int blockAlign = channels * (bitsPerSample / 8);
        int fmtSize = 16;
        int totalRiffSize = 4 + (8 + fmtSize) + (8 + dataBytes.Length);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(totalRiffSize);
        writer.Write("WAVE"u8.ToArray());

        writer.Write("fmt "u8.ToArray());
        writer.Write(fmtSize);
        writer.Write((ushort)audioFormat);
        writer.Write((ushort)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((ushort)blockAlign);
        writer.Write((ushort)bitsPerSample);

        writer.Write("data"u8.ToArray());
        writer.Write(dataBytes.Length);
        writer.Write(dataBytes);
    }
}
