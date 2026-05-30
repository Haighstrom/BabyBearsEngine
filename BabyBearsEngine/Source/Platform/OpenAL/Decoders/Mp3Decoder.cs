using System.IO;
using NLayer;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Decoder for MP3 files via the managed <c>NLayer</c> library. Fully decodes the file into a
/// single 16-bit PCM buffer at load time — matching the all-in-memory model used by the rest of
/// the audio subsystem. Streaming playback for very long music tracks is tracked as a separate
/// follow-up.
/// </summary>
internal static class Mp3Decoder
{
    /// <summary>
    /// Decodes the MP3 file at <paramref name="path"/> into a <see cref="DecodedAudio"/>.
    /// Throws <see cref="FileNotFoundException"/> if the file is missing,
    /// <see cref="NotSupportedException"/> if the channel layout is not mono/stereo.
    /// </summary>
    public static DecodedAudio Decode(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("MP3 file not found.", path);
        }

        using MpegFile mpeg = new(path);

        int channels = mpeg.Channels;
        int sampleRate = mpeg.SampleRate;
        TimeSpan duration = mpeg.Duration;

        ALFormat format = PcmConvert.GetSoundFormat16(channels, path);

        // NLayer's ReadSamples returns interleaved float samples in the standard [-1, 1] range.
        // Drive the read loop in fixed-size chunks (matching the Ogg decoder's strategy) and stitch
        // them into one contiguous buffer at the end. ReadSamples returns 0 when the stream ends.
        int frameSamples = channels * Math.Max(1, sampleRate / 5);
        float[] readBuffer = new float[frameSamples];

        List<float[]> chunks = [];
        List<int> chunkLengths = [];
        int totalSamples = 0;

        int readCount;
        while ((readCount = mpeg.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
        {
            float[] chunk = new float[readCount];
            Array.Copy(readBuffer, 0, chunk, 0, readCount);
            chunks.Add(chunk);
            chunkLengths.Add(readCount);
            totalSamples += readCount;
        }

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
