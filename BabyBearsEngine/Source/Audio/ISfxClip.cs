namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// A sound-effect clip — short-form audio intended to be played on one of the engine's pool of SFX
/// channels. Returned by <see cref="Audio.LoadSfx(string)"/>. Distinct from <see cref="IMusicClip"/>
/// so that <c>Audio.PlayMusic(sfxClip)</c> is a compile-time error rather than a runtime surprise.
/// </summary>
public interface ISfxClip : IAudioClip
{
}
