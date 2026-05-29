namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// A music clip — long-form audio intended for the dedicated music channel and playlist machinery.
/// Returned by <see cref="Audio.LoadMusic(string)"/>. Distinct from <see cref="ISfxClip"/> so that
/// <c>Audio.PlaySfx(musicTrack)</c> is a compile-time error rather than a runtime surprise.
/// </summary>
public interface IMusicClip : IAudioClip
{
    /// <summary>
    /// Human-readable name for the track, used for display in jukeboxes / "now playing" UI and for
    /// name-based lookup via <see cref="IPlaylist.Play(string)"/>. Defaults to the source file name
    /// without extension when not explicitly supplied at load time.
    /// </summary>
    string TrackName { get; }
}
