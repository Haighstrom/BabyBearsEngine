using System.IO;
using Concentus;
using Concentus.Oggfile;
using Concentus.Structs;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Decoder for Opus files (.opus) via the managed <c>Concentus</c> codec plus
/// <c>Concentus.OggFile</c> for Ogg framing. Fully decodes the file into a single 16-bit PCM buffer
/// at load time, matching the all-in-memory model used by the rest of the audio subsystem.
/// Streaming playback for very long music tracks is tracked as a separate follow-up.
///
/// Opus is always decoded at 48 kHz — that's the only rate Opus's internal decoders are spec'd to
/// produce at full quality. Mono/stereo channel count is extracted from the OpusHead packet ahead
/// of opening the decoder; the Concentus.OggFile wrapper does not expose channel count itself.
/// </summary>
internal static class OpusDecoder
{
    private const int OpusOutputSampleRate = 48000;

    /// <summary>
    /// Decodes the Opus file at <paramref name="path"/> into a <see cref="DecodedAudio"/>.
    /// Throws <see cref="FileNotFoundException"/> if the file is missing,
    /// <see cref="NotSupportedException"/> if the channel layout is not mono/stereo.
    /// </summary>
    public static DecodedAudio Decode(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Opus file not found.", path);
        }

        int channels = PeekOpusHeadChannels(path);
        ALFormat format = PcmConvert.GetSoundFormat16(channels, path);

        IOpusDecoder decoder = OpusCodecFactory.CreateDecoder(OpusOutputSampleRate, channels);

        using FileStream fileStream = File.OpenRead(path);
        OpusOggReadStream reader = new(decoder, fileStream);

        // The reader exposes TotalTime only when seekable, which a FileStream is. Use it for the
        // initial duration hint; we recompute against the actual decoded sample count at the end so
        // games can rely on Duration matching the bytes that were uploaded.
        TimeSpan reportedDuration = reader.TotalTime;

        List<short[]> chunks = [];
        int totalSamples = 0;

        while (reader.HasNextPacket)
        {
            short[]? chunk = reader.DecodeNextPacket();
            if (chunk is null || chunk.Length == 0)
            {
                continue;
            }
            chunks.Add(chunk);
            totalSamples += chunk.Length;
        }

        short[] interleaved = new short[totalSamples];
        int writeOffset = 0;
        foreach (short[] chunk in chunks)
        {
            Array.Copy(chunk, 0, interleaved, writeOffset, chunk.Length);
            writeOffset += chunk.Length;
        }

        byte[] pcm = PcmConvert.ShortInterleavedToPcm16(interleaved, totalSamples);

        TimeSpan duration = totalSamples > 0
            ? TimeSpan.FromSeconds((double)(totalSamples / channels) / OpusOutputSampleRate)
            : reportedDuration;

        return new DecodedAudio(pcm, format, OpusOutputSampleRate, duration);
    }

    /// <summary>
    /// Scans the start of an Opus file for the OpusHead magic and returns the channel count from
    /// the header. The Ogg page framing means OpusHead doesn't sit at a fixed offset — Ogg pages
    /// start with "OggS" and carry their payload after a variable-length segment table — so we
    /// search forward for the magic bytes within the first few KB rather than parse the framing.
    /// In practice OpusHead is always the first packet on the first page, well within 4 KB.
    /// </summary>
    private static int PeekOpusHeadChannels(string path)
    {
        const int peekBytes = 4096;
        byte[] head = new byte[peekBytes];
        int read;
        using (FileStream stream = File.OpenRead(path))
        {
            read = stream.Read(head, 0, peekBytes);
        }

        return ExtractOpusHeadChannels(head.AsSpan(0, read), path);
    }

    /// <summary>
    /// Extracts the channel count from a buffer that should contain an OpusHead packet somewhere in
    /// its first few bytes. Exposed at internal visibility so the unit tests can exercise the parse
    /// without having to write a real Ogg/Opus file to disk.
    /// </summary>
    internal static int ExtractOpusHeadChannels(ReadOnlySpan<byte> head, string path)
    {
        ReadOnlySpan<byte> magic = "OpusHead"u8;
        for (int i = 0; i <= head.Length - magic.Length - 1; i++)
        {
            if (head.Slice(i, magic.Length).SequenceEqual(magic))
            {
                // OpusHead layout: 8 bytes magic, 1 byte version, 1 byte channel count, ...
                if (i + 9 >= head.Length)
                {
                    break;
                }
                return head[i + 9];
            }
        }

        throw new NotSupportedException(
            $"Opus file '{path}' does not contain a recognisable OpusHead packet in the first {head.Length} bytes.");
    }
}
