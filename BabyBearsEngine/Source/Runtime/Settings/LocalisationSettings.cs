namespace BabyBearsEngine;

/// <summary>
/// Configuration for the localisation subsystem — where to load translation tables from,
/// which locale to activate at startup, and which locale to fall back to when a string is
/// missing in the current one.
/// </summary>
/// <remarks>
/// If <see cref="AssetsFolder"/> does not exist on disk when the engine starts,
/// <see cref="GameLauncher"/> leaves the default <see cref="NullLocaliser"/> installed and
/// every <see cref="Strings.Get"/> call returns its key verbatim. To enable real localisation,
/// create the folder and add one <c>{locale}.json</c> file per locale (flat
/// <c>{"key":"value"}</c> dictionaries).
/// </remarks>
public record class LocalisationSettings()
{
    /// <summary>The default localisation settings — <c>Assets/Localisation/</c>, English default and fallback.</summary>
    public static LocalisationSettings Default => new();

    /// <summary>
    /// Folder containing <c>{locale}.json</c> translation files. Resolved relative to the
    /// application's working directory. Defaults to <c>"Assets/Localisation/"</c>.
    /// </summary>
    public string AssetsFolder { get; init; } = "Assets/Localisation/";

    /// <summary>
    /// Locale activated when the engine starts. Must match the name of one of the JSON files
    /// in <see cref="AssetsFolder"/>. Defaults to <c>"en"</c>.
    /// </summary>
    public string DefaultLocale { get; init; } = "en";

    /// <summary>
    /// Locale consulted when the current locale has no entry for a requested key. Setting
    /// this to <see cref="DefaultLocale"/> disables fallback. Defaults to <c>"en"</c>.
    /// </summary>
    public string FallbackLocale { get; init; } = "en";
}
