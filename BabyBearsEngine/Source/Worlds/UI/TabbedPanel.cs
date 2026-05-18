using System.Collections.Generic;
using System.Collections.ObjectModel;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A UI panel with a row of tab buttons along the top. Clicking a tab switches the content
/// area to show that tab's items and hides all others.
/// </summary>
public class TabbedPanel : Entity
{
    private readonly List<Tab> _tabs = [];
    private readonly float _tabSpacing;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Total width in pixels.</param>
    /// <param name="height">Total height in pixels (tab bar + content area).</param>
    /// <param name="tabHeight">Height of the tab button row in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="tabSpacing">Horizontal gap between adjacent tab buttons. Defaults to 0.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public TabbedPanel(float x, float y, float width, float height, float tabHeight, TabbedPanelTheme theme, float tabSpacing = 0, int layer = 0)
        : base(x, y, width, height, layer: layer)
    {
        _tabSpacing = tabSpacing;

        float panelH = height - tabHeight;
        ContentPanel = new Entity(0, tabHeight, width, panelH);
        ContentPanel.Add(theme.PanelBackgroundFactory(new Rect(0, 0, width, panelH)));
        Add(ContentPanel);
    }

    /// <param name="rect">Total position and size relative to the parent container (tab bar + content area).</param>
    /// <param name="tabHeight">Height of the tab button row in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="tabSpacing">Horizontal gap between adjacent tab buttons. Defaults to 0.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public TabbedPanel(Rect rect, float tabHeight, TabbedPanelTheme theme, float tabSpacing = 0, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, tabHeight, theme, tabSpacing, layer)
    {
    }

    /// <summary>The content area entity. Add non-tab content here directly if needed.</summary>
    public Entity ContentPanel { get; }

    /// <summary>The currently active tab, or <c>null</c> if no tabs have been added.</summary>
    public Tab? CurrentTab { get; private set; }

    /// <summary>All tabs in the order they were added.</summary>
    public ReadOnlyCollection<Tab> Tabs => _tabs.AsReadOnly();

    /// <summary>Raised whenever the active tab changes.</summary>
    public event EventHandler<TabChangedEventArgs>? TabChanged;

    /// <summary>
    /// Adds a tab to the panel. The tab button is positioned after any existing tabs.
    /// </summary>
    /// <param name="tab">The tab to add.</param>
    /// <param name="activate">
    /// When <c>true</c>, immediately switches to this tab. When <c>false</c> (default), the
    /// tab is added in the deactivated state; if no tab is currently active, it is activated
    /// automatically so the panel is never left empty.
    /// </param>
    public void AddTab(Tab tab, bool activate = false)
    {
        _tabs.Add(tab);
        Add(tab);
        tab.Attach(ContentPanel);
        tab.LeftClicked += OnTabClicked;

        RepositionTabs();

        bool shouldActivate = activate || CurrentTab is null;

        if (shouldActivate)
        {
            Tab? previous = CurrentTab;
            CurrentTab?.Deactivate();
            CurrentTab = tab;
            tab.Activate();
            TabChanged?.Invoke(this, new TabChangedEventArgs(previous, tab));
        }
        else
        {
            tab.Deactivate();
        }
    }

    /// <summary>
    /// Removes a tab from the panel. Its content items are removed from the content area.
    /// If the removed tab was active, the adjacent tab (preferring the next one) becomes active.
    /// </summary>
    public void RemoveTab(Tab tab)
    {
        int index = _tabs.IndexOf(tab);

        if (index < 0)
        {
            return;
        }

        tab.LeftClicked -= OnTabClicked;
        tab.Detach();
        tab.Remove();
        _tabs.RemoveAt(index);

        RepositionTabs();

        if (tab != CurrentTab)
        {
            return;
        }

        if (_tabs.Count == 0)
        {
            CurrentTab = null;
            return;
        }

        Tab next = _tabs[Math.Min(index, _tabs.Count - 1)];
        Tab? previous = CurrentTab;
        CurrentTab = next;
        next.Activate();
        TabChanged?.Invoke(this, new TabChangedEventArgs(previous, next));
    }

    /// <summary>Switches the active tab. Does nothing if <paramref name="tab"/> is already active.</summary>
    public void SwitchTo(Tab tab)
    {
        if (!_tabs.Contains(tab))
        {
            throw new ArgumentException($"This TabbedPanel does not contain the given tab.", nameof(tab));
        }

        if (tab == CurrentTab)
        {
            return;
        }

        Tab? previous = CurrentTab;
        CurrentTab?.Deactivate();
        CurrentTab = tab;
        tab.Activate();
        TabChanged?.Invoke(this, new TabChangedEventArgs(previous, tab));
    }

    private void OnTabClicked(object? sender, EventArgs _)
    {
        if (sender is Tab tab)
        {
            SwitchTo(tab);
        }
    }

    private void RepositionTabs()
    {
        float x = 0;

        foreach (Tab tab in _tabs)
        {
            tab.X = x;
            tab.Y = 0;
            x += tab.Width + _tabSpacing;
        }
    }
}
