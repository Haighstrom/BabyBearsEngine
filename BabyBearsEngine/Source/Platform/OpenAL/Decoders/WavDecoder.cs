using System.IO;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Decoder for the RIFF/WAVE container with linear-PCM payload — the subset 99% of game-audio
/// pipelines emit. Scans the RIFF chunk tree, ignores everything except <c>fmt </c> and
/// <c>data</c>, and returns a <see cref="DecodedAudio"/> the OpenAL channel can upload directly.
/// </summary>
internal static class WavDecoder
{
    // Subset of WAVE_FORMAT_* values from mmreg.h. We only accept linear PCM here — anything else
    // (IEEE float, A-law, mu-law, ADPCM, ...) would need additional decoding logic.
    //
    // WAVE_FORMAT_EXTENSIBLE wraps a sub-format identified by a GUID; whether the file's actually
    // linear PCM in that case is determined by the first four bytes of the SubFormat GUID (which
    // mirror the basic WAVE_FORMAT_* code). Most modern WAV encoders — including the mixkit / SFX
    // exporters and several DAWs — emit EXTENSIBLE PCM rather than plain PCM, so accepting it is
    // important for real-world content.
    private const ushort WaveFormatPcm = 1;
    private const ushort WaveFormatExtensible = 0xFFFE;

    /// <summary>
    /// Decodes the WAV file at <paramref name="path"/> into a <see cref="DecodedAudio"/>.
    /// Throws <see cref="FileNotFoundException"/> if the file is missing,
    /// <see cref="NotSupportedException"/> if the file is not a linear-PCM WAV the decoder can handle.
    /// </summary>
    public static DecodedAudio Decode(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("WAV file not found.", path);
        }

        using FileStream stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);

        ReadRiffHeader(reader);

        ushort channelCount = 0;
        ushort bitsPerSample = 0;
        int sampleRate = 0;
        byte[]? pcm = null;

        // Walk the RIFF subchunks until we've seen both fmt and data. Chunk order isn't strictly
        // fixed (a LIST/INFO chunk before data is very common from common editors), so we have to
        // scan rather than assume positions.
        while (stream.Position < stream.Length)
        {
            string chunkId = new(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();
            long chunkStart = stream.Position;

            switch (chunkId)
            {
                case "fmt ":
                    ushort audioFormat = reader.ReadUInt16();
                    channelCount = reader.ReadUInt16();
                    sampleRate = reader.ReadInt32();
                    _ = reader.ReadInt32();  // byte rate — derivable from the others, ignored
                    _ = reader.ReadUInt16(); // block align
                    bitsPerSample = reader.ReadUInt16();

                    if (audioFormat == WaveFormatExtensible)
                    {
                        // Read the 22-byte extension. cbSize must be ≥ 22 for the SubFormat GUID to
                        // be present; the GUID's first four bytes mirror the basic WAVE_FORMAT_* code.
                        ushort cbSize = reader.ReadUInt16();
                        if (cbSize < 22 || chunkSize < 40)
                        {
                            throw new NotSupportedException(
                                $"WAV file '{path}' declares WAVE_FORMAT_EXTENSIBLE but the fmt extension is truncated (cbSize={cbSize}, chunkSize={chunkSize}).");
                        }
                        _ = reader.ReadUInt16(); // wValidBitsPerSample
                        _ = reader.ReadUInt32(); // dwChannelMask
                        uint subFormatCode = reader.ReadUInt32();
                        // Remaining 12 bytes of the GUID are the fixed suffix common to all KS subtypes;
                        // the chunk-position advance below skips past whatever's left.
                        audioFormat = (ushort)subFormatCode;
                    }

                    if (audioFormat != WaveFormatPcm)
                    {
                        throw new NotSupportedException(
                            $"WAV file '{path}' uses audio format {audioFormat}; only linear PCM (format 1, or EXTENSIBLE with PCM sub-format) is supported.");
                    }
                    break;

                case "data":
                    pcm = reader.ReadBytes(chunkSize);
                    if (pcm.Length != chunkSize)
                    {
                        throw new EndOfStreamException(
                            $"WAV file '{path}' declared a {chunkSize}-byte data chunk but only {pcm.Length} bytes were available.");
                    }
                    break;

                default:
                    // Unknown / uninteresting chunk — skip its payload entirely.
                    break;
            }

            // Always advance to the end of the declared chunk, even if we read fewer bytes than its
            // size (the fmt chunk sometimes has trailing extension bytes we ignore). RIFF chunks
            // are word-aligned: a chunk with an odd size has a padding byte that does NOT count
            // toward the declared size, so advance an extra byte when chunkSize is odd.
            stream.Position = chunkStart + chunkSize + (chunkSize & 1);
        }

        if (channelCount == 0 || bitsPerSample == 0)
        {
            throw new NotSupportedException($"WAV file '{path}' is missing a 'fmt ' chunk.");
        }
        if (pcm is null)
        {
            throw new NotSupportedException($"WAV file '{path}' is missing a 'data' chunk.");
        }

        // OpenAL's stable formats only cover 8 and 16-bit samples. 24-bit PCM is a common bit depth
        // for modern editors and SFX exporters (the mixkit pack, Audacity defaults, several DAWs),
        // so we downconvert to 16-bit at decode time rather than rejecting it. The conversion just
        // drops the lowest byte of each 3-byte sample, which is a clean truncation of the bottom
        // 8 bits — audible noise floor only, no perceptible quality loss for typical game audio.
        // 32-bit PCM (rare but real) could be handled the same way; deferred until we see content
        // that needs it.
        TimeSpan duration = ComputeDuration(pcm.Length, channelCount, bitsPerSample, sampleRate);
        if (bitsPerSample == 24)
        {
            pcm = Convert24BitTo16Bit(pcm);
            bitsPerSample = 16;
        }

        ALFormat format = GetSoundFormat(channelCount, bitsPerSample);

        return new DecodedAudio(pcm, format, sampleRate, duration);
    }

    private static byte[] Convert24BitTo16Bit(byte[] pcm24)
    {
        // 24-bit little-endian: each sample is 3 bytes (low, mid, high). The high two bytes already
        // form a valid 16-bit little-endian sample once the low byte is dropped — equivalent to
        // dividing the original 24-bit value by 256 with truncation toward zero.
        int sampleCount = pcm24.Length / 3;
        byte[] pcm16 = new byte[sampleCount * 2];
        for (int i = 0; i < sampleCount; i++)
        {
            pcm16[i * 2] = pcm24[i * 3 + 1];
            pcm16[i * 2 + 1] = pcm24[i * 3 + 2];
        }
        return pcm16;
    }

    /// <summary>
    /// Maps (channels, bits) onto the OpenAL sample format. Supports mono and stereo at 8 or 16 bits.
    /// Throws <see cref="NotSupportedException"/> for any other combination (5.1, 32-bit float, etc.).
    /// </summary>
    public static ALFormat GetSoundFormat(int channels, int bits)
    {
        return (channels, bits) switch
        {
            (1, 8) => ALFormat.Mono8,
            (1, 16) => ALFormat.Mono16,
            (2, 8) => ALFormat.Stereo8,
            (2, 16) => ALFormat.Stereo16,
            _ => throw new NotSupportedException(
                $"Unsupported WAV format: {channels} channels at {bits} bits per sample. Only mono/stereo at 8 or 16 bits are supported."),
        };
    }

    private static void ReadRiffHeader(BinaryReader reader)
    {
        string signature = new(reader.ReadChars(4));
        if (signature != "RIFF")
        {
            throw new NotSupportedException($"Not a RIFF file (signature was '{signature}').");
        }

        _ = reader.ReadInt32(); // RIFF chunk size — we use stream length instead

        string format = new(reader.ReadChars(4));
        if (format != "WAVE")
        {
            throw new NotSupportedException($"Not a WAVE file (form type was '{format}').");
        }
    }

    private static TimeSpan ComputeDuration(int byteCount, int channels, int bitsPerSample, int sampleRate)
    {
        if (sampleRate <= 0 || channels <= 0 || bitsPerSample <= 0)
        {
            return TimeSpan.Zero;
        }

        int bytesPerSample = bitsPerSample / 8;
        long sampleFrames = byteCount / (channels * bytesPerSample);
        return TimeSpan.FromSeconds((double)sampleFrames / sampleRate);
    }
}
