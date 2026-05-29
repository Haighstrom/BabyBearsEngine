namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// High-level state of the music playback subsystem. Sound effects do not contribute to this state —
/// it reflects only what <see cref="Audio.Playlist"/> is currently doing.
/// </summary>
public enum AudioState
{
    /// <summary>No music is playing and the playlist is empty or has finished.</summary>
    Stopped = 0,

    /// <summary>A music track is actively playing.</summary>
    Playing = 1,

    /// <summary>Playback is paused; calling <see cref="Audio.Resume"/> will resume from the current position.</summary>
    Paused = 2,
}
