using System.IO;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Decoder for Ogg Vorbis files via the managed <c>NVorbis</c> library. Fully decodes the file into
/// a single 16-bit PCM buffer at load time — matching the all-in-memory model used by the rest of
/// the audio subsystem. Streaming playback for very long music tracks is tracked as a separate
/// follow-up; for typical game music (a few minutes), the in-memory footprint is acceptable
/// (e.g. ~10 MB for a 60 s stereo track at 44.1 kHz).
/// </summary>
internal static class OggDecoder
{
    /// <summary>
    /// Decodes the Ogg Vorbis file at <paramref name="path"/> into a <see cref="DecodedAudio"/>.
    /// Throws <see cref="FileNotFoundException"/> if the file is missing,
    /// <see cref="NotSupportedException"/> if the channel layout is not mono/stereo, and lets
    /// NVorbis exceptions surface for malformed Vorbis streams.
    /// </summary>
    public static DecodedAudio Decode(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Ogg Vorbis file not found.", path);
        }

        using VorbisReader reader = new(path);

        int channels = reader.Channels;
        int sampleRate = reader.SampleRate;
        TimeSpan duration = reader.TotalTime;

        ALFormat format = PcmConvert.GetSoundFormat16(channels, path);

        // NVorbis reports total samples per channel via TotalSamples; multiplying by channel count
        // gives the interleaved sample count we need to allocate. Real-world streams can occasionally
        // produce a slightly different sample count to the header total (rounding in chained streams),
        // so we treat TotalSamples as a starting hint and let the read loop drive actual allocation
        // through an incremental list-of-buffers strategy.
        int frameSamples = channels * Math.Max(1, sampleRate / 5);
        float[] readBuffer = new float[frameSamples];

        List<float[]> chunks = [];
        List<int> chunkLengths = [];
        int totalSamples = 0;

        int readCount;
        while ((readCount = reader.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
        {
            float[] chunk = new float[readCount];
            Array.Copy(readBuffer, 0, chunk, 0, readCount);
            chunks.Add(chunk);
            chunkLengths.Add(readCount);
            totalSamples += readCount;
        }

        // Flatten chunks into a single contiguous float buffer, then convert to 16-bit PCM in one
        // pass. Going chunk-by-chunk would save a transient float allocation but at the cost of
        // duplicating the conversion loop; the flatten step is cheap (one memcpy per chunk) and
        // keeps PcmConvert as the single conversion site.
        float[] interleaved = new float[totalSamples];
        int writeOffset = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            Array.Copy(chunks[i], 0, interleaved, writeOffset, chunkLengths[i]);
            writeOffset += chunkLengths[i];
        }

        byte[] pcm = PcmConvert.FloatInterleavedToPcm16(interleaved, totalSamples);

        return new DecodedAudio(pcm, format, sampleRate, duration);
    }
}
