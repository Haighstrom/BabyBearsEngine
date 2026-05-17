namespace BabyBearsEngine.Worlds.UI.Themes;

/// <summary>
/// Visual styling for a <see cref="DropdownList{T}"/>: one <see cref="ButtonTheme"/> for the
/// always-visible header (the closed-state button showing the current selection) and one for
/// the option rows shown in the open list.
/// </summary>
public sealed record DropdownListTheme
{
    private static readonly Colour s_defaultHeader = new(70, 100, 190);
    private static readonly Colour s_defaultOption = new(55, 80, 165);

    /// <summary>Styling for the header button — shown in both open and closed states.</summary>
    public required ButtonTheme Header { get; init; }

    /// <summary>Styling for each option button in the open list.</summary>
    public required ButtonTheme Option { get; init; }

    /// <summary>Bland placeholder theme — blue header, slightly darker blue options. Prototyping only.</summary>
    public static readonly DropdownListTheme Default = FromColours(s_defaultHeader, s_defaultOption);

    /// <summary>Builds a theme with solid-colour header and option buttons.</summary>
    public static DropdownListTheme FromColours(Colour header, Colour option) => new()
    {
        Header = ButtonTheme.FromColour(header),
        Option = ButtonTheme.FromColour(option),
    };
}
