namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// Service interface for the audio subsystem — the playback engine sitting behind the
/// <see cref="Audio"/> static facade. Tests substitute an implementation here via
/// <c>EngineConfiguration.AudioService</c>.
/// </summary>
public interface IAudio : IDisposable
{
    /// <summary>
    /// True if the audio subsystem initialised successfully and is ready to play. False when no
    /// audio device is available, when the OpenAL runtime is missing, or when
    /// <see cref="AudioSettings.Disabled"/> was set at startup. All play / load calls are no-ops
    /// when this is false; loaders return placeholder clips that do nothing.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>Current state of music playback. Sound effects do not influence this value.</summary>
    AudioState MusicState { get; }

    /// <summary>
    /// Master volume multiplier applied to both music and SFX. Range 0..1; values outside this range
    /// are clamped. Default 1.
    /// </summary>
    float MasterVolume { get; set; }

    /// <summary>Volume multiplier for music, applied on top of <see cref="MasterVolume"/>. Range 0..1.</summary>
    float MusicVolume { get; set; }

    /// <summary>Volume multiplier for sound effects, applied on top of <see cref="MasterVolume"/>. Range 0..1.</summary>
    float SfxVolume { get; set; }

    /// <summary>
    /// The music playlist. See <see cref="IPlaylist"/> for navigation, name-based lookup, and event hooks.
    /// </summary>
    IPlaylist Playlist { get; }

    /// <summary>
    /// Loads a music track from disk and returns a clip suitable for the music channel. Throws
    /// <see cref="System.IO.FileNotFoundException"/> if the file is missing,
    /// <see cref="NotSupportedException"/> if the format is not recognised. The
    /// <see cref="IMusicClip.TrackName"/> defaults to the file name without extension.
    /// </summary>
    IMusicClip LoadMusic(string path);

    /// <summary>
    /// Loads a music track from disk with an explicit display name. Use the path-only overload when
    /// the file name is fine as the display name.
    /// </summary>
    IMusicClip LoadMusic(string path, string trackName);

    /// <summary>
    /// Loads a sound effect from disk. Throws <see cref="System.IO.FileNotFoundException"/> if the
    /// file is missing, <see cref="NotSupportedException"/> if the format is not recognised.
    /// </summary>
    ISfxClip LoadSfx(string path);

    /// <summary>
    /// Convenience for <see cref="Playlist"/>.SetTracks + Play with a single track. Replaces any
    /// currently-set playlist.
    /// </summary>
    void PlayMusic(IMusicClip clip);

    /// <summary>
    /// Convenience for <see cref="Playlist"/>.SetTracks + Play with multiple tracks. Replaces any
    /// currently-set playlist.
    /// </summary>
    void PlayMusic(IReadOnlyList<IMusicClip> playlist);

    /// <summary>
    /// Like <see cref="PlayMusic(IMusicClip)"/>, but switches smoothly: any music already playing fades out over
    /// the first half of <paramref name="fadeDuration"/>, then <paramref name="clip"/> starts and fades in over
    /// the second half. If nothing is playing it simply fades in over the second half. A zero or negative duration
    /// switches immediately. A later call (including a plain <c>PlayMusic</c> or <see cref="StopMusic"/>)
    /// cancels any fade still in progress. The fade scales the music gain on top of <see cref="MusicVolume"/>.
    /// </summary>
    void PlayMusic(IMusicClip clip, TimeSpan fadeDuration);

    /// <summary>
    /// Like <see cref="PlayMusic(IReadOnlyList{IMusicClip})"/>, but with the same fade-out / fade-in switch as
    /// <see cref="PlayMusic(IMusicClip, TimeSpan)"/>. Fading applies only to the switch itself, not to the
    /// advance from one playlist track to the next.
    /// </summary>
    void PlayMusic(IReadOnlyList<IMusicClip> playlist, TimeSpan fadeDuration);

    /// <summary>
    /// Plays <paramref name="clip"/> on the first free SFX channel. If all channels are busy the
    /// clip is dropped (a warning is logged).
    /// </summary>
    void PlaySfx(ISfxClip clip);

    /// <summary>
    /// Plays <paramref name="clip"/> with a per-shot volume multiplier (0..1) applied on top of the
    /// usual master × sfx volume. Useful for "quiet variant" plays without mutating any persistent state.
    /// </summary>
    void PlaySfx(ISfxClip clip, float volumeOverride);

    /// <summary>Pauses both music and any currently-playing sound effects. Idempotent.</summary>
    void Pause();

    /// <summary>Resumes after a <see cref="Pause"/>. Idempotent.</summary>
    void Resume();

    /// <summary>Stops the music track and clears the playlist head, leaving the playlist contents intact.</summary>
    void StopMusic();

    /// <summary>Stops any currently-playing sound effects. Does not affect music.</summary>
    void StopSfx();

    /// <summary>Stops everything — music and SFX both.</summary>
    void StopAll();

    /// <summary>Fires when <see cref="MusicState"/> changes.</summary>
    event Action<AudioStateChangedEventArgs>? MusicStateChanged;
}
