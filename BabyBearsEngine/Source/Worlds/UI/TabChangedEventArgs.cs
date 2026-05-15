namespace BabyBearsEngine.Worlds.UI;

/// <summary>Payload for <see cref="TabbedPanel.TabChanged"/>.</summary>
public sealed class TabChangedEventArgs(Tab? previous, Tab current) : EventArgs
{
    /// <summary>The tab that was active before the switch, or <c>null</c> if no tab was active.</summary>
    public Tab? Previous { get; } = previous;

    /// <summary>The tab that is now active.</summary>
    public Tab Current { get; } = current;
}
