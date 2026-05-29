using BabyBearsEngine.AudioSystem;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Sentinel clip used when the audio subsystem is disabled or failed to initialise. Holds metadata
/// (path, track name) so display UI still works, but never allocates AL resources and is silent on
/// playback. Returned from <see cref="OpenALAudioService.LoadMusic(string)"/> /
/// <see cref="OpenALAudioService.LoadSfx(string)"/> when <c>IsAvailable</c> is false.
/// </summary>
internal sealed class NoOpMusicClip(string path, string trackName) : IMusicClip
{
    public string Path { get; } = path;
    public TimeSpan Duration => TimeSpan.Zero;
    public string TrackName { get; } = trackName;
    public void Dispose() { }
}

internal sealed class NoOpSfxClip(string path) : ISfxClip
{
    public string Path { get; } = path;
    public TimeSpan Duration => TimeSpan.Zero;
    public void Dispose() { }
}
