using BabyBearsEngine.AudioSystem;

namespace BabyBearsEngine;

/// <summary>
/// Configuration for the audio subsystem. Controls initial volumes, the size of the SFX channel
/// pool, and the music playlist's default loop/shuffle behaviour. Pass via
/// <see cref="ApplicationSettings.AudioSettings"/>.
/// </summary>
public record class AudioSettings()
{
    /// <summary>The default audio settings — all subsystems at their defaults.</summary>
    public static AudioSettings Default => new();

    /// <summary>
    /// A preset that skips audio init entirely. Use for headless tests or CI environments where no
    /// audio device is available. With this set, <see cref="IAudio.IsAvailable"/> is false and all
    /// play / load calls are silent no-ops.
    /// </summary>
    public static AudioSettings Silent => new() { Disabled = true };

    /// <summary>
    /// When true, no OpenAL device is opened and the audio subsystem runs in a no-op mode. Useful
    /// for tests, CI, and headless environments. Defaults to false.
    /// </summary>
    public bool Disabled { get; init; } = false;

    /// <summary>
    /// Number of simultaneous sound-effect channels. Each channel can play one SFX at a time;
    /// additional SFX while all channels are busy are dropped with a warning. Defaults to 15.
    /// </summary>
    public int MaxSfxChannels { get; init; } = 15;

    /// <summary>Initial value for <see cref="IAudio.MasterVolume"/>. Range 0..1. Defaults to 1.</summary>
    public float MasterVolume { get; init; } = 1f;

    /// <summary>Initial value for <see cref="IAudio.MusicVolume"/>. Range 0..1. Defaults to 1.</summary>
    public float MusicVolume { get; init; } = 1f;

    /// <summary>Initial value for <see cref="IAudio.SfxVolume"/>. Range 0..1. Defaults to 1.</summary>
    public float SfxVolume { get; init; } = 1f;

    /// <summary>Initial value for <see cref="IPlaylist.Loop"/>. Defaults to true.</summary>
    public bool LoopMusic { get; init; } = true;

    /// <summary>Initial value for <see cref="IPlaylist.Shuffle"/>. Defaults to true.</summary>
    public bool ShuffleMusic { get; init; } = true;
}
