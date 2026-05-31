using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.LocalisationDemo;

/// <summary>
/// Walks through the localisation API:
/// <list type="bullet">
///   <item><c>Strings.Get(key)</c> for plain lookup,</item>
///   <item><c>Strings.Format(key, args)</c> for placeholder substitution,</item>
///   <item><c>Strings.SetLocale(code)</c> to switch the active locale,</item>
///   <item><c>Strings.LocaleChanged</c> to refresh any pre-built <see cref="TextGraphic"/> whose source string came from a translation lookup.</item>
/// </list>
/// Translation tables live in <c>Assets/Localisation/{locale}.json</c> — <see cref="GameLauncher"/>
/// auto-installs a <see cref="JsonLocaliser"/> when that folder exists.
/// </summary>
internal class LocalisationDemoWorld : DemoWorld
{
    private const int CoinCount = 7;
    private const string CoinsKey = "shop.coins";
    private const string ExtraChars = "èéà¡üçñ";
    private const string FallbackKey = "fallback.demo";
    private const string GreetingKey = "menu.greeting";
    private const string OptionsKey = "menu.options";
    private const string PlayKey = "menu.play";
    private const string QuitKey = "menu.quit";

    // Layout
    private const int FirstRowY = 200;
    private const int KeyColumnRight = 380;
    private const int KeyColumnWidth = 260;
    private const int RowGap = 6;
    private const int RowHeight = 28;
    private const int ValueColumnLeft = 400;
    private const int ValueColumnWidth = 360;

    private static readonly FontDefinition s_bodyFont = new("Times New Roman", 14, ExtraCharactersToLoad: ExtraChars);
    private static readonly FontDefinition s_keyFont = new("Times New Roman", 14, FontStyle.Italic, ExtraCharactersToLoad: ExtraChars);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 22, FontStyle.Bold);

    private static readonly (string Label, string Code)[] s_locales =
    [
        ("English",  "en"),
        ("Français", "fr"),
        ("Español",  "es"),
        ("Deutsch",  "de"),
    ];

    private readonly TextGraphic _localeIndicator;
    private readonly List<LocalisedRow> _rows = new();

    public override string Name => "Localisation Demo";

    public LocalisationDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(245, 245, 250);

        Add(new TextGraphic(s_titleFont, "Localisation Demo", new Colour(30, 30, 30),
            0f, 50f, Window.Width, 30f)
        {
            HAlignment = HAlignment.Centred,
        });

        Add(new TextGraphic(s_bodyFont,
            "Click a button below to switch locale. Every \"value\" line re-resolves via Strings.LocaleChanged.",
            new Colour(80, 80, 80), 40f, 100f, Window.Width - 80, 22f)
        {
            HAlignment = HAlignment.Centred,
        });

        _localeIndicator = new TextGraphic(s_keyFont, BuildLocaleIndicator(), new Colour(40, 40, 100),
            0f, 140f, Window.Width, 22f)
        {
            HAlignment = HAlignment.Centred,
        };
        Add(_localeIndicator);

        int rowY = FirstRowY;
        AddRow(GreetingKey, formatted: false, ref rowY);
        AddRow(PlayKey, formatted: false, ref rowY);
        AddRow(OptionsKey, formatted: false, ref rowY);
        AddRow(QuitKey, formatted: false, ref rowY);
        AddRow(CoinsKey, formatted: true, ref rowY);
        AddRow(FallbackKey, formatted: false, ref rowY);

        AddLocaleButtons();

        Strings.LocaleChanged += OnLocaleChanged;
    }

    public override void Unload()
    {
        Strings.LocaleChanged -= OnLocaleChanged;
        base.Unload();
    }

    private void AddRow(string key, bool formatted, ref int rowY)
    {
        Add(new TextGraphic(s_keyFont, key, new Colour(120, 120, 120),
            KeyColumnRight - KeyColumnWidth, rowY, KeyColumnWidth, RowHeight)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        TextGraphic value = new(s_bodyFont, ResolveValue(key, formatted),
            new Colour(20, 20, 20),
            ValueColumnLeft, rowY, ValueColumnWidth, RowHeight)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(value);

        _rows.Add(new LocalisedRow(key, value, formatted));
        rowY += RowHeight + RowGap;
    }

    private void AddLocaleButtons()
    {
        // The default ButtonTheme uses a font atlas that doesn't include 'ç' or '¡', so the
        // "Français" / "Español" labels would crash at render time. Swap the theme's font for
        // one with the locale-button glyphs explicitly loaded.
        ButtonTheme theme = ButtonTheme.FromColour(new Colour(100, 160, 220)) with
        {
            Text = TextTheme.Default with
            {
                Font = new FontDefinition("Times New Roman", 16, ExtraCharactersToLoad: ExtraChars),
            },
        };

        const float buttonWidth = 130f;
        const float buttonHeight = 40f;
        const float buttonGap = 20f;
        const float buttonY = 480f;

        float totalWidth = s_locales.Length * buttonWidth + (s_locales.Length - 1) * buttonGap;
        float startX = (Window.Width - totalWidth) / 2f;

        for (int i = 0; i < s_locales.Length; i++)
        {
            (string label, string code) = s_locales[i];
            float x = startX + i * (buttonWidth + buttonGap);

            Button button = new(x, buttonY, buttonWidth, buttonHeight, theme, label);
            button.LeftClicked += (_, _) => Strings.SetLocale(code);
            Add(button);
        }
    }

    private static string BuildLocaleIndicator() =>
        $"Current locale: {Strings.CurrentLocale}    (available: {string.Join(", ", Strings.AvailableLocales)})";

    private void OnLocaleChanged(LocaleChangedEventArgs args)
    {
        _localeIndicator.Text = BuildLocaleIndicator();

        foreach (LocalisedRow row in _rows)
        {
            row.Display.Text = ResolveValue(row.Key, row.Formatted);
        }
    }

    private static string ResolveValue(string key, bool formatted) =>
        formatted ? Strings.Format(key, CoinCount) : Strings.Get(key);

    private sealed record LocalisedRow(string Key, TextGraphic Display, bool Formatted);
}
