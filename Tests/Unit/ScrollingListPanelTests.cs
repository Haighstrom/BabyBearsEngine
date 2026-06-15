using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ScrollingListPanelTests
{
    private sealed class FakeWindow : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) => entity.Parent = null;
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    // CalculateThumbProportion

    [TestMethod]
    public void CalculateThumbProportion_ContentEqualsPanel_ReturnsOne()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 300f);

        Assert.AreEqual(1f, result);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentTwicePanel_ReturnsHalf()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 600f);

        Assert.AreEqual(0.5f, result);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentThreeTimes_ReturnsThird()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(100f, 300f);

        Assert.AreEqual(1f / 3f, result, delta: 0.0001f);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentSmallerThanPanel_ReturnsOne()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 100f);

        Assert.AreEqual(1f, result);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentZero_ReturnsOne()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 0f);

        Assert.AreEqual(1f, result);
    }

    // CalculateScrollOffset

    [TestMethod]
    public void CalculateScrollOffset_AtZero_ReturnsZero()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(0f, 300f, 600f);

        Assert.AreEqual(0f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_AtOne_ReturnsMaxOffset()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(1f, 300f, 600f);

        Assert.AreEqual(300f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_AtHalf_ReturnsHalfMaxOffset()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(0.5f, 300f, 600f);

        Assert.AreEqual(150f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_ContentSmallerThanPanel_ReturnsZero()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(1f, 300f, 100f);

        Assert.AreEqual(0f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_ContentEqualsPanel_ReturnsZero()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(1f, 300f, 300f);

        Assert.AreEqual(0f, result);
    }

    // ContentPane.GetWindowCoordinates — scroll-aware hit positioning

    [TestMethod]
    public void ContentPane_WithScrollOffset_GetWindowCoordinatesSubtractsScrollFromY()
    {
        // ContentPane at local (50, 100). FakeWindow is its parent and returns coords unchanged.
        var pane = new ScrollingListPanel.ContentPane(50f, 100f, 200f, 300f)
        {
            Parent = new FakeWindow(),
            ScrollOffset = 150f,
        };

        // Item at local (0, 50) inside the pane → expected screen Y = 100 + 50 - 150 = 0
        var (_, y) = pane.GetWindowCoordinates(0f, 50f);
        Assert.AreEqual(0f, y);
    }

    [TestMethod]
    public void ContentPane_WithScrollOffset_XCoordinateIsUnaffected()
    {
        var pane = new ScrollingListPanel.ContentPane(50f, 100f, 200f, 300f)
        {
            Parent = new FakeWindow(),
            ScrollOffset = 150f,
        };

        var (x, _) = pane.GetWindowCoordinates(20f, 50f);
        // X is always panel.X + localX, regardless of vertical scroll.
        Assert.AreEqual(70f, x);
    }

    [TestMethod]
    public void ContentPane_ZeroScroll_GetWindowCoordinatesAddsPaneOffset()
    {
        var pane = new ScrollingListPanel.ContentPane(50f, 100f, 200f, 300f)
        {
            Parent = new FakeWindow(),
            ScrollOffset = 0f,
        };

        var (x, y) = pane.GetWindowCoordinates(20f, 50f);
        Assert.AreEqual(70f, x);
        Assert.AreEqual(150f, y);
    }

    // GetUnscrolledWindowPosition — the scissor anchor must not move with scroll, since the
    // viewport itself stays put while only the content inside it scrolls. Regression test for
    // a bug where the scissor used the scroll-adjusted GetWindowCoordinates and so the
    // clipped area drifted off the panel as the user scrolled, leaving items visible outside
    // the panel bounds.

    [TestMethod]
    public void ContentPane_GetUnscrolledWindowPosition_IgnoresScrollOffset()
    {
        var pane = new ScrollingListPanel.ContentPane(50f, 100f, 200f, 300f)
        {
            Parent = new FakeWindow(),
            ScrollOffset = 150f,
        };

        var (x, y) = pane.GetUnscrolledWindowPosition();

        Assert.AreEqual(50f, x);
        Assert.AreEqual(100f, y);
    }

    [TestMethod]
    public void ContentPane_GetUnscrolledWindowPosition_MatchesZeroScrollGetWindowCoordinates()
    {
        var pane = new ScrollingListPanel.ContentPane(50f, 100f, 200f, 300f)
        {
            Parent = new FakeWindow(),
            ScrollOffset = 0f,
        };

        var (gwcX, gwcY) = pane.GetWindowCoordinates(0f, 0f);
        var (unscrolledX, unscrolledY) = pane.GetUnscrolledWindowPosition();

        Assert.AreEqual(gwcX, unscrolledX);
        Assert.AreEqual(gwcY, unscrolledY);
    }

    // Theme — ScrollbarWidth

    [TestMethod]
    public void Theme_ScrollbarWidth_DefaultsTo20()
    {
        Assert.AreEqual(20f, ScrollingListPanelTheme.Default.ScrollbarWidth);
    }

    [TestMethod]
    public void Theme_ScrollbarWidth_IsOverridable()
    {
        ScrollingListPanelTheme theme = ScrollingListPanelTheme.Default with { ScrollbarWidth = 32f };

        Assert.AreEqual(32f, theme.ScrollbarWidth);
    }
}
