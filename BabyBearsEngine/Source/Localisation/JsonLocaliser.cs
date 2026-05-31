using System.Globalization;
using System.IO;
using BabyBearsEngine.IO;

namespace BabyBearsEngine;

/// <summary>
/// JSON-backed <see cref="ILocaliser"/>. Loads every <c>*.json</c> file in the supplied assets
/// folder at construction; each file's name without extension is treated as the locale code,
/// and the file's contents must deserialise to a flat <c>Dictionary&lt;string, string&gt;</c>.
/// </summary>
/// <remarks>
/// <para><see cref="Get"/> falls back: requested key in the current locale → same key in the
/// fallback locale → the key itself. <see cref="Format"/> uses the
/// <see cref="CultureInfo"/> for the current locale where one is registered (so numbers,
/// dates, and percentages format in the local style); falls back to
/// <see cref="CultureInfo.InvariantCulture"/> when the locale code isn't a recognised culture.</para>
///
/// <para>Locale codes are matched case-insensitively, both at file load time and when
/// <see cref="SetLocale"/> is called.</para>
/// </remarks>
public sealed class JsonLocaliser : ILocaliser
{
    private readonly string _fallbackLocale;
    private readonly Dictionary<string, Dictionary<string, string>> _tables = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLocale;

    /// <param name="assetsFolder">Directory containing one <c>{locale}.json</c> file per supported locale.</param>
    /// <param name="defaultLocale">Locale to activate on startup. Must have a matching file in <paramref name="assetsFolder"/>.</param>
    /// <param name="fallbackLocale">Locale consulted when the current locale has no entry for a requested key.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="assetsFolder"/> does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="defaultLocale"/> has no JSON file in <paramref name="assetsFolder"/>.</exception>
    public JsonLocaliser(string assetsFolder, string defaultLocale, string fallbackLocale)
    {
        ArgumentNullException.ThrowIfNull(assetsFolder);
        ArgumentNullException.ThrowIfNull(defaultLocale);
        ArgumentNullException.ThrowIfNull(fallbackLocale);

        if (!Files.DirectoryExists(assetsFolder))
        {
            throw new DirectoryNotFoundException($"Localisation assets folder not found: '{assetsFolder}'.");
        }

        foreach (string filePath in Files.GetFiles(assetsFolder, includeSubDirectories: false, searchPattern: "*.json"))
        {
            string localeCode = Path.GetFileNameWithoutExtension(filePath);
            Dictionary<string, string> entries = Json.Deserialize<Dictionary<string, string>>(Files.ReadText(filePath));
            _tables[localeCode] = entries;
        }

        AvailableLocales = [.. _tables.Keys];

        if (!_tables.ContainsKey(defaultLocale))
        {
            throw new ArgumentException(
                $"Default locale '{defaultLocale}' has no JSON file in '{assetsFolder}'. Available: [{string.Join(", ", AvailableLocales)}].",
                nameof(defaultLocale));
        }

        _currentLocale = defaultLocale;
        _fallbackLocale = fallbackLocale;
    }

    public string CurrentLocale => _currentLocale;

    public IReadOnlyList<string> AvailableLocales { get; }

    public event Action<LocaleChangedEventArgs>? LocaleChanged;

    public string Format(string key, params object[] args)
    {
        string template = Get(key);

        if (args.Length == 0)
        {
            return template;
        }

        return string.Format(ResolveCulture(_currentLocale), template, args);
    }

    public string Get(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_tables.TryGetValue(_currentLocale, out Dictionary<string, string>? current)
            && current.TryGetValue(key, out string? value))
        {
            return value;
        }

        if (!string.Equals(_currentLocale, _fallbackLocale, StringComparison.OrdinalIgnoreCase)
            && _tables.TryGetValue(_fallbackLocale, out Dictionary<string, string>? fallback)
            && fallback.TryGetValue(key, out string? fallbackValue))
        {
            return fallbackValue;
        }

        return key;
    }

    public void SetLocale(string locale)
    {
        ArgumentNullException.ThrowIfNull(locale);

        if (!_tables.ContainsKey(locale))
        {
            throw new ArgumentException(
                $"Locale '{locale}' is not available. Available: [{string.Join(", ", AvailableLocales)}].",
                nameof(locale));
        }

        if (string.Equals(_currentLocale, locale, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string oldLocale = _currentLocale;
        _currentLocale = locale;
        LocaleChanged?.Invoke(new LocaleChangedEventArgs(oldLocale, locale));
    }

    private static CultureInfo ResolveCulture(string locale)
    {
        try
        {
            return CultureInfo.GetCultureInfo(locale);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}
