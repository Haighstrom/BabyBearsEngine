namespace BabyBearsEngine.AudioSystem;

/// <summary>
/// Payload for <see cref="IAudio.MusicStateChanged"/>. Fires whenever the music subsystem transitions
/// between <see cref="AudioState.Playing"/>, <see cref="AudioState.Paused"/>, and <see cref="AudioState.Stopped"/>.
/// </summary>
public sealed class AudioStateChangedEventArgs(AudioState oldState, AudioState newState)
{
    /// <summary>The state the music subsystem was in before this transition.</summary>
    public AudioState OldState { get; } = oldState;

    /// <summary>The state the music subsystem is in after this transition.</summary>
    public AudioState NewState { get; } = newState;
}
