namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// The music playlist owned by the audio subsystem. Accessed via <see cref="Audio.Playlist"/>.
/// Holds an ordered list of <see cref="IMusicClip"/> tracks and the playback head; advances to the
/// next track automatically as each one finishes (subject to <see cref="Loop"/> and <see cref="Shuffle"/>).
/// </summary>
public interface IPlaylist
{
    /// <summary>
    /// The tracks currently in the playlist, in the order they were supplied to <see cref="SetTracks(IReadOnlyList{IMusicClip})"/>.
    /// This order is stable: <see cref="Shuffle"/> changes the playback sequence internally but never reorders this list.
    /// </summary>
    IReadOnlyList<IMusicClip> Tracks { get; }

    /// <summary>
    /// Index of the track currently playing (or last played, if paused/stopped). Returns -1 when the
    /// playlist is empty or has never been played.
    /// </summary>
    int CurrentIndex { get; }

    /// <summary>The track at <see cref="CurrentIndex"/>, or null if no track is current.</summary>
    IMusicClip? CurrentTrack { get; }

    /// <summary>
    /// When true (the default), the playlist restarts from the first track after the last one
    /// finishes. When false, playback stops and <see cref="IAudio.MusicState"/> transitions to
    /// <see cref="AudioState.Stopped"/>. Initial value comes from <see cref="AudioSettings.LoopMusic"/>.
    /// </summary>
    bool Loop { get; set; }

    /// <summary>
    /// When true, the playlist is reshuffled on each new pass (and at <c>Play()</c> time). When
    /// shuffling repeats the playlist, the engine avoids starting the new pass with the same track
    /// that just ended. Initial value comes from <see cref="AudioSettings.ShuffleMusic"/>.
    /// </summary>
    bool Shuffle { get; set; }

    /// <summary>Replaces the playlist contents. Does not change playback state; call <see cref="Play()"/> to start.</summary>
    void SetTracks(IReadOnlyList<IMusicClip> tracks);

    /// <summary>Replaces the playlist contents. Does not change playback state; call <see cref="Play()"/> to start.</summary>
    void SetTracks(params IMusicClip[] tracks);

    /// <summary>Empties the playlist and stops playback.</summary>
    void Clear();

    /// <summary>
    /// Starts (or restarts) playback at the current head position — index 0 if nothing has played yet,
    /// or the head set by a previous <see cref="NextTrack"/> / <see cref="Play(int)"/>. No-op if the
    /// playlist is empty.
    /// </summary>
    void Play();

    /// <summary>
    /// Jumps to and plays the track at <paramref name="index"/>. Wraps modulo <see cref="Tracks"/>.Count
    /// when the index is out of range. No-op if the playlist is empty.
    /// </summary>
    void Play(int index);

    /// <summary>
    /// Jumps to and plays the first track whose <see cref="IMusicClip.TrackName"/> matches
    /// <paramref name="trackName"/> (case-insensitive). Throws <see cref="KeyNotFoundException"/> if
    /// no track matches.
    /// </summary>
    void Play(string trackName);

    /// <summary>
    /// Jumps to and plays <paramref name="clip"/>. The clip must already be present in the playlist;
    /// throws <see cref="ArgumentException"/> otherwise.
    /// </summary>
    void Play(IMusicClip clip);

    /// <summary>
    /// Advances to the next track and plays it. Wraps to the first track if at the end (regardless
    /// of <see cref="Loop"/>; <c>Loop</c> only controls the automatic post-end behaviour). No-op if
    /// the playlist is empty.
    /// </summary>
    void NextTrack();

    /// <summary>
    /// Goes back to the previous track and plays it. Wraps to the last track if at the start. No-op
    /// if the playlist is empty.
    /// </summary>
    void PreviousTrack();

    /// <summary>
    /// Fires whenever the playlist begins playing a different track — including the very first track
    /// after a <see cref="Play()"/> call.
    /// </summary>
    event Action<MusicTrackChangedEventArgs>? TrackChanged;
}
