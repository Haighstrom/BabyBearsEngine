using BabyBearsEngine.AudioSystem;

namespace BabyBearsEngine;

/// <summary>
/// Static facade over the installed <see cref="IAudio"/> service. Mirrors the surface of
/// <see cref="IAudio"/> with no extra logic — all members route through
/// <c>EngineConfiguration.AudioService</c>; tests substitute a fake there.
///
/// Music navigation (next/previous track, name lookup, track listing) lives on
/// <see cref="Playlist"/>; play / volume / stop convenience methods live directly on this class.
/// </summary>
public static class Audio
{
    private static IAudio Implementation => EngineConfiguration.AudioService;

    /// <inheritdoc cref="IAudio.IsAvailable"/>
    public static bool IsAvailable => Implementation.IsAvailable;

    /// <inheritdoc cref="IAudio.MusicState"/>
    public static AudioState MusicState => Implementation.MusicState;

    /// <inheritdoc cref="IAudio.Playlist"/>
    public static IPlaylist Playlist => Implementation.Playlist;

    /// <inheritdoc cref="IAudio.MasterVolume"/>
    public static float MasterVolume
    {
        get => Implementation.MasterVolume;
        set => Implementation.MasterVolume = value;
    }

    /// <inheritdoc cref="IAudio.MusicVolume"/>
    public static float MusicVolume
    {
        get => Implementation.MusicVolume;
        set => Implementation.MusicVolume = value;
    }

    /// <inheritdoc cref="IAudio.SfxVolume"/>
    public static float SfxVolume
    {
        get => Implementation.SfxVolume;
        set => Implementation.SfxVolume = value;
    }

    /// <inheritdoc cref="IAudio.LoadMusic(string)"/>
    public static IMusicClip LoadMusic(string path) => Implementation.LoadMusic(path);

    /// <inheritdoc cref="IAudio.LoadMusic(string, string)"/>
    public static IMusicClip LoadMusic(string path, string trackName) => Implementation.LoadMusic(path, trackName);

    /// <inheritdoc cref="IAudio.LoadSfx(string)"/>
    public static ISfxClip LoadSfx(string path) => Implementation.LoadSfx(path);

    /// <inheritdoc cref="IAudio.PlayMusic(IMusicClip)"/>
    public static void PlayMusic(IMusicClip clip) => Implementation.PlayMusic(clip);

    /// <summary>
    /// Convenience for setting a multi-track playlist and starting playback.
    /// </summary>
    public static void PlayMusic(params IMusicClip[] playlist) => Implementation.PlayMusic(playlist);

    /// <inheritdoc cref="IAudio.PlayMusic(IReadOnlyList{IMusicClip})"/>
    public static void PlayMusic(IReadOnlyList<IMusicClip> playlist) => Implementation.PlayMusic(playlist);

    /// <inheritdoc cref="IAudio.PlaySfx(ISfxClip)"/>
    public static void PlaySfx(ISfxClip clip) => Implementation.PlaySfx(clip);

    /// <inheritdoc cref="IAudio.PlaySfx(ISfxClip, float)"/>
    public static void PlaySfx(ISfxClip clip, float volumeOverride) => Implementation.PlaySfx(clip, volumeOverride);

    /// <inheritdoc cref="IAudio.Pause"/>
    public static void Pause() => Implementation.Pause();

    /// <inheritdoc cref="IAudio.Resume"/>
    public static void Resume() => Implementation.Resume();

    /// <inheritdoc cref="IAudio.StopMusic"/>
    public static void StopMusic() => Implementation.StopMusic();

    /// <inheritdoc cref="IAudio.StopSfx"/>
    public static void StopSfx() => Implementation.StopSfx();

    /// <inheritdoc cref="IAudio.StopAll"/>
    public static void StopAll() => Implementation.StopAll();

    /// <summary>
    /// Fires when <see cref="MusicState"/> changes. Subscribers route through whichever
    /// <see cref="IAudio"/> implementation is currently installed.
    /// </summary>
    public static event Action<AudioStateChangedEventArgs>? MusicStateChanged
    {
        add => Implementation.MusicStateChanged += value;
        remove => Implementation.MusicStateChanged -= value;
    }
}
