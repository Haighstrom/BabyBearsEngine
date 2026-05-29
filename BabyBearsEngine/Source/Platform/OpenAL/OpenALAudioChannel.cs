using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// A single OpenAL source — one playback slot. Holds no audio data of its own; clips own the AL
/// buffers and channels just bind a buffer at play time. Used both for the dedicated music channel
/// and for each entry in the SFX channel pool.
/// </summary>
internal sealed class OpenALAudioChannel : IDisposable
{
    private readonly int _source;
    private OpenALAudioClip? _currentClip = null;
    private bool _disposed = false;

    public OpenALAudioChannel()
    {
        _source = AL.GenSource();
        AL.Source(_source, ALSourcef.Gain, 1f);
        AL.Source(_source, ALSourceb.Looping, false);
    }

    /// <summary>The clip currently bound to this channel, or null if the channel is idle.</summary>
    public OpenALAudioClip? CurrentClip => _currentClip;

    /// <summary>True when the channel is not currently playing anything.</summary>
    public bool IsFree => GetState() is ALSourceState.Initial or ALSourceState.Stopped;

    /// <summary>True if the channel has finished playing the clip it was given (vs still playing or paused).</summary>
    public bool HasFinished => _currentClip is not null && GetState() == ALSourceState.Stopped;

    /// <summary>Binds <paramref name="clip"/> to this channel at the given gain and starts playback.</summary>
    public void Play(OpenALAudioClip clip, float gain)
    {
        AL.SourceStop(_source);
        AL.Source(_source, ALSourcei.Buffer, clip.BufferId);
        AL.Source(_source, ALSourcef.Gain, gain);
        AL.SourcePlay(_source);
        _currentClip = clip;
    }

    /// <summary>
    /// Pauses playback if currently playing. No-op if already paused or stopped. Resume() resumes
    /// from this point.
    /// </summary>
    public void Pause()
    {
        if (GetState() == ALSourceState.Playing)
        {
            AL.SourcePause(_source);
        }
    }

    /// <summary>Resumes playback if currently paused. No-op otherwise.</summary>
    public void Resume()
    {
        if (GetState() == ALSourceState.Paused)
        {
            AL.SourcePlay(_source);
        }
    }

    /// <summary>Stops playback and unbinds the current clip. Idempotent.</summary>
    public void Stop()
    {
        AL.SourceStop(_source);
        // Detach the buffer so the clip can be disposed safely while this channel is idle.
        AL.Source(_source, ALSourcei.Buffer, 0);
        _currentClip = null;
    }

    /// <summary>Updates the channel's gain. Range 0..1; values outside this range are clamped by AL.</summary>
    public void SetGain(float gain)
    {
        AL.Source(_source, ALSourcef.Gain, gain);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        AL.SourceStop(_source);
        AL.DeleteSource(_source);
        _currentClip = null;
        _disposed = true;
    }

    private ALSourceState GetState()
    {
        return (ALSourceState)AL.GetSource(_source, ALGetSourcei.SourceState);
    }
}
