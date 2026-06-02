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

    /// <summary>
    /// Clip lifetime is managed by the audio service: it tracks every clip it loads and frees
    /// them all on its own <see cref="OpenALAudioService.Dispose"/>. Calling <c>Dispose</c> on
    /// an individual clip is a no-op — otherwise a user-initiated <c>AL.DeleteBuffer</c> while
    /// the buffer is still bound to a playing source would fail silently (OpenAL specifies
    /// <c>AL_INVALID_OPERATION</c> and leaves the buffer alive but orphaned).
    /// </summary>
    public void Dispose()
    {
        // Intentionally a no-op for public callers; the audio service calls Destroy() on
        // teardown, by which point every channel has already been stopped and unbound.
    }

    /// <summary>Frees the underlying AL buffer. Called by <see cref="OpenALAudioService.Dispose"/> after every channel has been stopped.</summary>
    internal void Destroy()
    {
        if (_disposed)
        {
            return;
        }

        AL.DeleteBuffer(_bufferId);
        OpenALErrorCheck.Check(nameof(AL.DeleteBuffer));
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
