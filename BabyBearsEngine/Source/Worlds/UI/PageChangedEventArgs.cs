namespace BabyBearsEngine.Worlds.UI;

/// <summary>Payload for <see cref="PagedPanel.PageChanged"/>.</summary>
public sealed class PageChangedEventArgs(int previousPage, int currentPage) : EventArgs
{
    /// <summary>Zero-based index of the page that was showing before the change.</summary>
    public int PreviousPage { get; } = previousPage;

    /// <summary>Zero-based index of the page now showing.</summary>
    public int CurrentPage { get; } = currentPage;
}
