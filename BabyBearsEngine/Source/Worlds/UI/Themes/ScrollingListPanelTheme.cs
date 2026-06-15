namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="ScrollingListPanel"/>: an optional background colour and a
/// <see cref="ScrollbarTheme"/> for the vertical scrollbar on the right-hand side.
/// </summary>
public sealed record ScrollingListPanelTheme
{
    private static readonly Colour s_defaultBackground = new(50, 50, 50);
    private static readonly Colour s_defaultTrack = new(35, 35, 35);
    private static readonly Colour s_defaultThumb = new(120, 120, 120);

    /// <summary>Optional background fill colour. <see langword="null"/> means no background is drawn.</summary>
    public Colour? BackgroundColour { get; init; } = null;

    /// <summary>Visual styling for the vertical scrollbar.</summary>
    public required ScrollbarTheme Scrollbar { get; init; }

    /// <summary>Width in pixels of the vertical scrollbar and the strip reserved for it on the right edge. Defaults to 20.</summary>
    public float ScrollbarWidth { get; init; } = 20f;

    /// <summary>Bland placeholder theme — dark background, darker track, mid-grey thumb. Prototyping only.</summary>
    public static readonly ScrollingListPanelTheme Default = new()
    {
        BackgroundColour = s_defaultBackground,
        Scrollbar = ScrollbarTheme.FromColours(s_defaultTrack, s_defaultThumb),
    };

    /// <summary>Builds a theme with solid-colour background and scrollbar.</summary>
    public static ScrollingListPanelTheme FromColours(Colour background, Colour track, Colour thumb) => new()
    {
        BackgroundColour = background,
        Scrollbar = ScrollbarTheme.FromColours(track, thumb),
    };
}
