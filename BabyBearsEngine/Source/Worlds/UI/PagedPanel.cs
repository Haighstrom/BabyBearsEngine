using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A UI panel that shows one of several content pages at a time, with previous/next arrow
/// buttons and a "{n} / {count}" page counter. Only the current page is kept active and
/// visible; the others are deactivated and hidden. Paging wraps around at both ends.
/// <para>The panel sits at its parent's origin and adds the pages unmodified, so page content
/// keeps whatever coordinates it was authored with. The arrow buttons and counter label are
/// positioned by the rects passed to the constructor and persist across page changes.</para>
/// </summary>
public class PagedPanel : Entity
{
    private int _currentPage = 0;
    private readonly TextGraphic _pageLabel;
    private readonly IReadOnlyList<Entity> _pages;

    /// <param name="pages">The content pages, shown one at a time in order. May be empty.</param>
    /// <param name="previousButton">Position and size of the "previous page" arrow button.</param>
    /// <param name="nextButton">Position and size of the "next page" arrow button.</param>
    /// <param name="pageLabel">Position and size of the "{n} / {count}" counter label.</param>
    /// <param name="previousButtonTheme">Visual styling for the "previous page" arrow button.</param>
    /// <param name="nextButtonTheme">Visual styling for the "next page" arrow button.</param>
    /// <param name="pageLabelTheme">Text styling for the counter label.</param>
    /// <param name="previousButtonText">Optional label drawn on the "previous page" button. Leave empty when the button theme already supplies an arrow texture.</param>
    /// <param name="nextButtonText">Optional label drawn on the "next page" button. Leave empty when the button theme already supplies an arrow texture.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public PagedPanel(
        IList<Entity> pages,
        Rect previousButton,
        Rect nextButton,
        Rect pageLabel,
        ButtonTheme previousButtonTheme,
        ButtonTheme nextButtonTheme,
        TextTheme pageLabelTheme,
        string previousButtonText = "",
        string nextButtonText = "",
        int layer = 0)
        : base(0, 0, 0, 0, layer: layer)
    {
        _pages = [.. pages];

        foreach (Entity page in _pages)
        {
            Add(page);
        }

        _pageLabel = new TextGraphic(pageLabelTheme, "", pageLabel, layer);
        Add(_pageLabel);

        // The arrows only earn their place when there is somewhere to page to. They're added as
        // children (so the panel owns them) and never referenced again, so locals suffice.
        if (_pages.Count > 1)
        {
            Button previousPageButton = new(previousButton, previousButtonTheme, previousButtonText, layer);
            previousPageButton.LeftClicked += (_, _) => PreviousPage();
            Add(previousPageButton);

            Button nextPageButton = new(nextButton, nextButtonTheme, nextButtonText, layer);
            nextPageButton.LeftClicked += (_, _) => NextPage();
            Add(nextPageButton);
        }

        ShowCurrentPage();
    }

    /// <summary>Zero-based index of the page currently showing.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>The number of pages held by the panel.</summary>
    public int PageCount => _pages.Count;

    /// <summary>Raised whenever the visible page changes.</summary>
    public event EventHandler<PageChangedEventArgs>? PageChanged;

    /// <summary>
    /// Switches to the page at <paramref name="index"/>, wrapping around if it falls outside
    /// the valid range. Does nothing if that page is already showing or the panel has no pages.
    /// </summary>
    public void GoToPage(int index)
    {
        if (_pages.Count == 0)
        {
            return;
        }

        int wrappedIndex = ((index % _pages.Count) + _pages.Count) % _pages.Count;

        if (wrappedIndex == _currentPage)
        {
            return;
        }

        int previousPage = _currentPage;
        _currentPage = wrappedIndex;
        ShowCurrentPage();
        PageChanged?.Invoke(this, new PageChangedEventArgs(previousPage, _currentPage));
    }

    /// <summary>Advances to the next page, wrapping from the last page back to the first.</summary>
    public void NextPage() => GoToPage(_currentPage + 1);

    /// <summary>Goes back to the previous page, wrapping from the first page to the last.</summary>
    public void PreviousPage() => GoToPage(_currentPage - 1);

    private void ShowCurrentPage()
    {
        for (int i = 0; i < _pages.Count; i++)
        {
            bool isCurrent = i == _currentPage;
            _pages[i].Active = isCurrent;
            _pages[i].Visible = isCurrent;
        }

        _pageLabel.Text = _pages.Count == 0 ? "0 / 0" : $"{_currentPage + 1} / {_pages.Count}";
    }
}
