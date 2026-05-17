namespace BabyBearsEngine.Worlds.UI;

/// <summary>Payload for <see cref="DropdownList{T}.SelectionChanged"/>.</summary>
/// <param name="OldValue">The previously selected value.</param>
/// <param name="NewValue">The newly selected value.</param>
public sealed record SelectionChangedEventArgs<T>(T OldValue, T NewValue);
