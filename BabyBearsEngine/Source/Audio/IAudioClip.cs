namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// Common base for clips loaded by <see cref="Audio.LoadMusic(string)"/> and
/// <see cref="Audio.LoadSfx(string)"/>. Most callers will work with the role-specific
/// <see cref="IMusicClip"/> and <see cref="ISfxClip"/> types rather than this interface directly —
/// distinct types prevent passing a music track to a SFX channel (or vice versa) by accident.
/// </summary>
public interface IAudioClip : IDisposable
{
    /// <summary>The absolute or relative path the clip was loaded from. Useful for diagnostics.</summary>
    string Path { get; }

    /// <summary>The decoded duration of the clip. Returns <see cref="TimeSpan.Zero"/> if unknown.</summary>
    TimeSpan Duration { get; }
}
