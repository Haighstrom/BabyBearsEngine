using System.Globalization;

namespace BabyBearsEngine;

/// <summary>
/// No-op <see cref="ILocaliser"/> installed by default. Returns every key verbatim — games
/// that haven't opted into localisation pay nothing, and any missing strings show up as their
/// keys rather than blank text. Refuses any <see cref="SetLocale"/> call that isn't equal to
/// <see cref="CurrentLocale"/>; install a <see cref="JsonLocaliser"/> to enable real switching.
/// </summary>
public sealed class NullLocaliser : ILocaliser
{
    private const string DefaultLocale = "en";

    public string CurrentLocale => DefaultLocale;

    public IReadOnlyList<string> AvailableLocales { get; } = [];

    public event Action<LocaleChangedEventArgs>? LocaleChanged
    {
        add { }
        remove { }
    }

    public string Format(string key, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(key);
        return args.Length == 0 ? key : string.Format(CultureInfo.InvariantCulture, key, args);
    }

    public string Get(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key;
    }

    public void SetLocale(string locale)
    {
        ArgumentNullException.ThrowIfNull(locale);

        if (locale != CurrentLocale)
        {
            throw new InvalidOperationException(
                $"No localisation service installed — cannot switch to locale '{locale}'. " +
                $"Configure {nameof(LocalisationSettings)} and add JSON files to the assets folder.");
        }
    }
}
