namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// Payload for <see cref="IPlaylist.TrackChanged"/>. Fires whenever the playlist moves on to a new
/// track — whether triggered by the previous track finishing, <see cref="IPlaylist.NextTrack"/>,
/// <see cref="IPlaylist.PreviousTrack"/>, or an explicit <c>Play(...)</c> call.
/// </summary>
public sealed class MusicTrackChangedEventArgs(IMusicClip? previous, IMusicClip current)
{
    /// <summary>The track that was playing before this change, or null if nothing was playing.</summary>
    public IMusicClip? Previous { get; } = previous;

    /// <summary>The track that started playing as a result of this change.</summary>
    public IMusicClip Current { get; } = current;
}
