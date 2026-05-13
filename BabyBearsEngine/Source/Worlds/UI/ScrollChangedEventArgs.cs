namespace BabyBearsEngine.Worlds.UI;

/// <summary>Payload for <see cref="Scrollbar.ScrollChanged"/>.</summary>
/// <param name="OldValue">The previous <see cref="Scrollbar.AmountFilled"/> value, in [0, 1].</param>
/// <param name="NewValue">The new <see cref="Scrollbar.AmountFilled"/> value, in [0, 1].</param>
public sealed record ScrollChangedEventArgs(float OldValue, float NewValue);
