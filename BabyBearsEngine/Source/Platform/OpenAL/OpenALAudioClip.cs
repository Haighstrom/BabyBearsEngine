using BabyBearsEngine.AudioSystem;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Concrete clip backed by a single OpenAL buffer holding the entire decoded audio. Used by both
/// <see cref="OpenALMusicClip"/> and <see cref="OpenALSfxClip"/>; the distinction at the public
/// API level is purely a type tag — playback mechanics are identical for short fully-loaded clips.
/// When streaming formats (Ogg etc.) arrive, music clips will need a separate streaming variant;
/// this all-in-memory implementation suffices for the WAV-only v1.
/// </summary>
internal abstract class OpenALAudioClip : IAudioClip
{
    private readonly int _bufferId;
    private bool _disposed = false;

    protected OpenALAudioClip(string path, DecodedAudio decoded)
    {
        Path = path;
        Duration = decoded.Duration;

        _bufferId = AL.GenBuffer();
        OpenALErrorCheck.Check(nameof(AL.GenBuffer));
        AL.BufferData(_bufferId, decoded.Format, decoded.Pcm, decoded.SampleRate);
        OpenALErrorCheck.Check(nameof(AL.BufferData));
    }

    public string Path { get; }

    public TimeSpan Duration { get; }

    /// <summary>The AL buffer holding this clip's PCM data. Bound to a source at play time.</summary>
    public int BufferId => _bufferId;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        AL.DeleteBuffer(_bufferId);
        _disposed = true;
    }
}

internal sealed class OpenALMusicClip(string path, string trackName, DecodedAudio decoded)
    : OpenALAudioClip(path, decoded), IMusicClip
{
    public string TrackName { get; } = trackName;
}

internal sealed class OpenALSfxClip(string path, DecodedAudio decoded)
    : OpenALAudioClip(path, decoded), ISfxClip
{
}
