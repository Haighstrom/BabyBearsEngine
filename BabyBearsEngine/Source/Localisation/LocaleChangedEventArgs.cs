namespace BabyBearsEngine;

/// <summary>
/// Payload for <see cref="ILocaliser.LocaleChanged"/>. Fires whenever
/// <see cref="ILocaliser.SetLocale"/> actually changes the active locale.
/// </summary>
public sealed class LocaleChangedEventArgs(string oldLocale, string newLocale)
{
    /// <summary>The locale that was active before the change.</summary>
    public string OldLocale { get; } = oldLocale;

    /// <summary>The locale that is active after the change.</summary>
    public string NewLocale { get; } = newLocale;
}
