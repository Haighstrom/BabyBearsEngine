namespace BabyBearsEngine;

/// <summary>
/// Static facade over the installed <see cref="ILocaliser"/> service. Every member is a pure
/// delegate to <c>EngineConfiguration.LocalisationService</c>, so substituting the underlying
/// implementation (the default <see cref="NullLocaliser"/>, a production
/// <see cref="JsonLocaliser"/>, or a custom fake) automatically reroutes every caller.
/// Mirrors <see cref="Randomisation"/> in shape.
/// </summary>
/// <remarks>
/// Inject <see cref="ILocaliser"/> directly when you need an object-level test seam; use this
/// facade in UI code, dialogue, and anywhere else strings are looked up by key.
/// </remarks>
public static class Strings
{
    private static ILocaliser Source => EngineConfiguration.LocalisationService;

    /// <inheritdoc cref="ILocaliser.CurrentLocale"/>
    public static string CurrentLocale => Source.CurrentLocale;

    /// <inheritdoc cref="ILocaliser.AvailableLocales"/>
    public static IReadOnlyList<string> AvailableLocales => Source.AvailableLocales;

    /// <summary>
    /// Subscribes to <see cref="ILocaliser.LocaleChanged"/> on the currently installed service.
    /// Note that subscribing here pins the subscription to the service installed at subscribe
    /// time; replacing the service does not migrate existing subscribers.
    /// </summary>
    public static event Action<LocaleChangedEventArgs>? LocaleChanged
    {
        add => Source.LocaleChanged += value;
        remove => Source.LocaleChanged -= value;
    }

    /// <inheritdoc cref="ILocaliser.Format(string, object[])"/>
    public static string Format(string key, params object[] args) => Source.Format(key, args);

    /// <inheritdoc cref="ILocaliser.Get(string)"/>
    public static string Get(string key) => Source.Get(key);

    /// <inheritdoc cref="ILocaliser.SetLocale(string)"/>
    public static void SetLocale(string locale) => Source.SetLocale(locale);
}
