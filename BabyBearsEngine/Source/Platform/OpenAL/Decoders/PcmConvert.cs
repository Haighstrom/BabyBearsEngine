using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Shared helpers for converting decoder outputs into the 16-bit signed-PCM byte layout that the
/// OpenAL channel uploads. Most managed audio decoders emit either float (normalised to [-1, 1])
/// or 16-bit interleaved short samples; this class collapses both paths to a single byte[] in
/// little-endian order, which is what <c>AL.BufferData</c> expects for Mono16/Stereo16 formats.
/// </summary>
internal static class PcmConvert
{
    /// <summary>
    /// Converts a buffer of interleaved float samples (range [-1, 1]) into the 16-bit little-endian
    /// byte layout OpenAL expects. Out-of-range values are clamped — float decoders very occasionally
    /// emit slightly over ±1.0 due to rounding, and unclamped values would wrap.
    /// </summary>
    public static byte[] FloatInterleavedToPcm16(float[] samples, int sampleCount)
    {
        byte[] pcm = new byte[sampleCount * 2];
        for (int i = 0; i < sampleCount; i++)
        {
            float clamped = samples[i];
            if (clamped > 1f)
            {
                clamped = 1f;
            }
            else if (clamped < -1f)
            {
                clamped = -1f;
            }

            short sample = (short)(clamped * short.MaxValue);
            pcm[i * 2] = (byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }
        return pcm;
    }

    /// <summary>
    /// Converts a buffer of interleaved short samples into the 16-bit little-endian byte layout
    /// OpenAL expects. Same semantics as a memcpy on little-endian hosts, but spelled out so the
    /// result is portable.
    /// </summary>
    public static byte[] ShortInterleavedToPcm16(short[] samples, int sampleCount)
    {
        byte[] pcm = new byte[sampleCount * 2];
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = samples[i];
            pcm[i * 2] = (byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }
        return pcm;
    }

    /// <summary>
    /// Maps (channels, 16-bit) onto the OpenAL sample format. The compressed-audio decoders all
    /// resolve to 16-bit at decode time, so we only need to disambiguate mono vs stereo here.
    /// Throws <see cref="NotSupportedException"/> for channel counts other than 1 or 2.
    /// </summary>
    public static ALFormat GetSoundFormat16(int channels, string path)
    {
        return channels switch
        {
            1 => ALFormat.Mono16,
            2 => ALFormat.Stereo16,
            _ => throw new NotSupportedException(
                $"Audio file '{path}' has {channels} channels; only mono and stereo are supported."),
        };
    }
}
