namespace BabyBearsEngine.Worlds.UI;

/// <summary>Payload for <see cref="CyclingValueButton{T}.ValueChanged"/>.</summary>
/// <param name="OldValue">The value before the change.</param>
/// <param name="NewValue">The value after the change.</param>
public sealed record CyclingValueChangedEventArgs<T>(T OldValue, T NewValue);
