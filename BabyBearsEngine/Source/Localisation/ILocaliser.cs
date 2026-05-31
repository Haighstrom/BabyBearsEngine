namespace BabyBearsEngine;

/// <summary>
/// Abstraction over the engine-wide localisation service. Resolve user-facing strings by key
/// rather than embedding English text at call sites so the same UI code works in every
/// supported locale.
/// </summary>
/// <remarks>
/// <para>Use <see cref="NullLocaliser"/> as the default — it returns every key verbatim, so a
/// game that hasn't opted into localisation pays nothing and a missing key shows up in-game
/// rather than as a blank string. Switch to <see cref="JsonLocaliser"/> by setting
/// <see cref="LocalisationSettings.AssetsFolder"/> and shipping JSON dictionaries under it.</para>
///
/// <para>Inject <see cref="ILocaliser"/> directly when a class needs an object-level test seam;
/// otherwise call <see cref="Strings"/>, which routes through
/// <see cref="EngineConfiguration.LocalisationService"/>.</para>
/// </remarks>
public interface ILocaliser
{
    /// <summary>BCP-47 code of the locale currently being served (e.g. <c>"en"</c>, <c>"fr"</c>).</summary>
    string CurrentLocale { get; }

    /// <summary>Every locale this implementation can switch to. Empty for <see cref="NullLocaliser"/>.</summary>
    IReadOnlyList<string> AvailableLocales { get; }

    /// <summary>
    /// Returns the translation for <paramref name="key"/> in the current locale, falling back
    /// to the fallback locale and finally to <paramref name="key"/> itself if no translation
    /// is found. Missing-key visibility is intentional — translators see exactly which strings
    /// still need work.
    /// </summary>
    string Get(string key);

    /// <summary>
    /// Looks up the translation for <paramref name="key"/> and runs <see cref="string.Format(System.IFormatProvider, string, object[])"/>
    /// against it with <paramref name="args"/>, using the <see cref="System.Globalization.CultureInfo"/> for the
    /// current locale where one exists so numbers and dates are formatted in the local style.
    /// </summary>
    string Format(string key, params object[] args);

    /// <summary>
    /// Switches the active locale. Fires <see cref="LocaleChanged"/> if the locale actually changes.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <paramref name="locale"/> is not in <see cref="AvailableLocales"/>.</exception>
    void SetLocale(string locale);

    /// <summary>
    /// Fires after <see cref="SetLocale"/> changes <see cref="CurrentLocale"/>. UI screens
    /// subscribe to this to refresh any pre-built <see cref="Worlds.Graphics.Text.TextGraphic"/>
    /// instances whose source string came from a translation lookup.
    /// </summary>
    event Action<LocaleChangedEventArgs>? LocaleChanged;
}
