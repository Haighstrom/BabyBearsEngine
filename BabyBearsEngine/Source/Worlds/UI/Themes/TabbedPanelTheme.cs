using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="TabbedPanel"/>: factories for the content-area background
/// and the two tab button states (active / inactive), plus the text style for tab labels.
/// </summary>
public sealed record TabbedPanelTheme
{
    private static readonly Colour s_defaultPanel = new(210, 210, 210);
    private static readonly Colour s_defaultActiveTab = new(230, 230, 230);
    private static readonly Colour s_defaultInactiveTab = new(160, 175, 190);

    /// <summary>
    /// Factory producing the background graphic for the content area. Called once with the
    /// content panel's local rectangle (origin (0, 0), panel's width and height).
    /// </summary>
    public required Func<Rect, IGraphic> PanelBackgroundFactory { get; init; }

    /// <summary>
    /// Factory producing the background graphic for an active (selected) tab button. Called
    /// once per tab, with the tab's local rectangle.
    /// </summary>
    public required Func<Rect, IGraphic> ActiveTabFactory { get; init; }

    /// <summary>
    /// Factory producing the background graphic for an inactive (unselected) tab button.
    /// Called once per tab, with the tab's local rectangle.
    /// </summary>
    public required Func<Rect, IGraphic> InactiveTabFactory { get; init; }

    /// <summary>Text styling applied to every tab label.</summary>
    public required TextTheme TabText { get; init; }

    /// <summary>Bland placeholder theme — grey tones, default text. For prototyping only.</summary>
    public static readonly TabbedPanelTheme Default = FromColours(s_defaultPanel, s_defaultActiveTab, s_defaultInactiveTab);

    /// <summary>Builds a theme using solid-colour backgrounds for the panel and both tab states.</summary>
    public static TabbedPanelTheme FromColours(Colour panel, Colour activeTab, Colour inactiveTab) => new()
    {
        PanelBackgroundFactory = r => new ColourGraphic(panel, r.X, r.Y, r.W, r.H),
        ActiveTabFactory = r => new ColourGraphic(activeTab, r.X, r.Y, r.W, r.H),
        InactiveTabFactory = r => new ColourGraphic(inactiveTab, r.X, r.Y, r.W, r.H),
        TabText = TextTheme.Default,
    };
}
