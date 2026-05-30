using System.IO;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Decoder for FLAC files via the managed <c>CUETools.Codecs.FLAKE-Reloaded</c> library. Fully
/// decodes the file into a single 16-bit PCM buffer at load time, matching the all-in-memory model
/// used by the rest of the audio subsystem. FLAC's native bit depths (16 or 24, occasionally higher)
/// are downsampled to 16-bit at decode time the same way <see cref="WavDecoder"/> handles 24-bit PCM.
/// Streaming playback for very long music tracks is tracked as a separate follow-up.
/// </summary>
internal static class FlacDecoder
{
    /// <summary>
    /// Decodes the FLAC file at <paramref name="path"/> into a <see cref="DecodedAudio"/>.
    /// Throws <see cref="FileNotFoundException"/> if the file is missing,
    /// <see cref="NotSupportedException"/> if the channel layout is not mono/stereo.
    /// </summary>
    public static DecodedAudio Decode(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("FLAC file not found.", path);
        }

        FlakeReader reader = new(path, null);
        try
        {
            return DecodeWith(reader, path);
        }
        finally
        {
            reader.Close();
        }
    }

    private static DecodedAudio DecodeWith(FlakeReader reader, string path)
    {
        AudioPCMConfig pcm = reader.PCM;
        int channels = pcm.ChannelCount;
        int sampleRate = pcm.SampleRate;
        int bitsPerSample = pcm.BitsPerSample;
        long totalFrames = reader.Length;

        ALFormat format = PcmConvert.GetSoundFormat16(channels, path);
        TimeSpan duration = sampleRate > 0
            ? TimeSpan.FromSeconds((double)totalFrames / sampleRate)
            : TimeSpan.Zero;

        // FLAC samples are int[,] indexed [frame, channel] in FLAKE. The value is a signed integer
        // whose magnitude is normalised to the file's bit depth (e.g. 16-bit means values in
        // [-32768, 32767]; 24-bit means [-8388608, 8388607]). We downconvert to 16-bit by right-
        // shifting away the extra precision — equivalent to dropping the low bits, same approach
        // as WavDecoder's 24-bit handling. Bit-depths below 16 (rare but legal in FLAC) get
        // left-shifted up to fill the 16-bit range.
        const int FramesPerRead = 65536;
        AudioBuffer buffer = new(pcm, (int)Math.Min(totalFrames, FramesPerRead));
        byte[] pcm16 = new byte[totalFrames * channels * 2];
        int writeOffset = 0;
        int shiftRight = Math.Max(0, bitsPerSample - 16);
        int shiftLeft = Math.Max(0, 16 - bitsPerSample);

        long framesRead = 0;
        while (framesRead < totalFrames)
        {
            // FLAKE's Read takes a maxLength cap. Passing the buffer's full size — bounded above
            // by FramesPerRead — drives one chunk per call; passing 0 would silently return zero
            // frames (the internal Prepare() applies Math.Min(size, maxLength)).
            int got = reader.Read(buffer, buffer.Size);
            if (got <= 0)
            {
                break;
            }

            int[,] samples = buffer.Samples;
            for (int frame = 0; frame < got; frame++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    int raw = samples[frame, channel];
                    int converted = shiftRight > 0 ? raw >> shiftRight : raw << shiftLeft;
                    if (converted > short.MaxValue)
                    {
                        converted = short.MaxValue;
                    }
                    else if (converted < short.MinValue)
                    {
                        converted = short.MinValue;
                    }
                    short sample = (short)converted;
                    pcm16[writeOffset++] = (byte)(sample & 0xFF);
                    pcm16[writeOffset++] = (byte)((sample >> 8) & 0xFF);
                }
            }
            framesRead += got;
        }

        // If the file's StreamInfo over-reports the frame count (rare but possible), trim the buffer
        // down to what we actually decoded. Under-reporting is handled implicitly by the while-loop
        // exit; over-reporting leaves trailing zeros that would just play silence — harmless, but
        // we trim for honesty.
        if (writeOffset != pcm16.Length)
        {
            byte[] trimmed = new byte[writeOffset];
            Array.Copy(pcm16, 0, trimmed, 0, writeOffset);
            pcm16 = trimmed;
        }

        return new DecodedAudio(pcm16, format, sampleRate, duration);
    }
}
