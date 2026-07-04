using BabyBearsEngine.Input;
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

    private sealed class FakeMouse : IMouse
    {
        public bool LeftDown { get; set; } = false;
        public bool MiddleDown { get; set; } = false;
        public bool RightDown { get; set; } = false;
        public bool LeftUp { get; set; } = true;
        public bool MiddleUp { get; set; } = true;
        public bool RightUp { get; set; } = true;
        public bool LeftPressed { get; set; } = false;
        public bool MiddlePressed { get; set; } = false;
        public bool RightPressed { get; set; } = false;
        public bool LeftReleased { get; set; } = false;
        public bool MiddleReleased { get; set; } = false;
        public bool RightReleased { get; set; } = false;
        public int ClientX { get; set; } = 5;
        public int ClientY { get; set; } = 5;
        public float WheelDelta { get; set; } = 0f;
        public int XDelta { get; set; } = 0;
        public int YDelta { get; set; } = 0;
        public bool ButtonDown(MouseButton button) => false;
        public bool ButtonPressed(MouseButton button) => false;
        public bool ButtonReleased(MouseButton button) => false;
        public bool AnyButtonDown(IEnumerable<MouseButton> buttons) => false;
        public bool AnyButtonDown(params MouseButton[] buttons) => false;
        public bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => false;
        public bool AnyButtonPressed(params MouseButton[] buttons) => false;
        public bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => false;
        public bool AnyButtonReleased(params MouseButton[] buttons) => false;
        public bool AllButtonsDown(IEnumerable<MouseButton> buttons) => false;
        public bool AllButtonsDown(params MouseButton[] buttons) => false;
        public bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => false;
        public bool AllButtonsPressed(params MouseButton[] buttons) => false;
        public bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => false;
        public bool AllButtonsReleased(params MouseButton[] buttons) => false;
    }

    // Test root that gives the panel a valid parent so it has a valid screen position.
    // GetWindowCoordinates returns the input unchanged — same as a World at the tree root.
    private sealed class FakeRoot : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private FakeMouse _mouse = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new FakeMouse();
        EngineConfiguration.MouseService = _mouse;
    }

    [TestCleanup]
    public void Cleanup()
    {
        EngineConfiguration.Reset();
        MouseSolver.Reset();
    }

    // Simulates one full frame: entity update (registers with MouseSolver) then MouseSolver.Update().
    private static void Frame(ScrollingListPanel panel, double elapsed = 0.016)
    {
        panel.Update(elapsed);
        MouseSolver.Update();
    }

    // Panel is 220x200 (paneWidth=200, scrollbarWidth=20, scrollbar occupies x=200..220).
    // ContentHeight=400 gives a max scroll offset of 200, so a 0.1 AmountFilled step moves
    // ScrollOffset by 20.
    private static ScrollingListPanel MakeScrollable(bool scrollOnMouseWheel = true)
    {
        ScrollingListPanel panel = new(220f, 200f, 20f, scrollOnMouseWheel)
        {
            Parent = new FakeRoot(),
            ContentHeight = 400f,
        };
        return panel;
    }

    // -------------------------------------------------------------------------
    // Mouse wheel scrolling over the content area (#289)

    [TestMethod]
    public void Wheel_OverContentArea_ScrollsThePanel()
    {
        ScrollingListPanel panel = MakeScrollable();
        _mouse.ClientX = 100;
        _mouse.ClientY = 100;
        Frame(panel); // mouse enters, mouseIsOver = true
        _mouse.WheelDelta = -1f;

        Frame(panel);

        Assert.AreEqual(20f, panel.ScrollOffset, delta: 0.001f);
    }

    [TestMethod]
    public void Wheel_OverScrollbarStrip_ScrollsExactlyOneStep()
    {
        // Regression test: both the panel and the embedded scrollbar intercept the wheel, but
        // MouseSolver only reports the top-most overlapping entity as moused-over, so only one
        // of them should react — never both (which would double the scroll step).
        ScrollingListPanel panel = MakeScrollable();
        _mouse.ClientX = 210;
        _mouse.ClientY = 100;
        Frame(panel);
        _mouse.WheelDelta = -1f;

        Frame(panel);

        Assert.AreEqual(20f, panel.ScrollOffset, delta: 0.001f);
    }

    [TestMethod]
    public void Wheel_OverContentArea_WhenScrollOnMouseWheelFalse_DoesNotScroll()
    {
        ScrollingListPanel panel = MakeScrollable(scrollOnMouseWheel: false);
        _mouse.ClientX = 100;
        _mouse.ClientY = 100;
        Frame(panel);
        _mouse.WheelDelta = -1f;

        Frame(panel);

        Assert.AreEqual(0f, panel.ScrollOffset);
    }

    [TestMethod]
    public void Wheel_OverScrollbarStrip_WhenPanelScrollOnMouseWheelFalse_StillScrolls()
    {
        // The panel-level flag only gates the panel's own content-area interception — the
        // embedded scrollbar keeps its own independent wheel handling regardless.
        ScrollingListPanel panel = MakeScrollable(scrollOnMouseWheel: false);
        _mouse.ClientX = 210;
        _mouse.ClientY = 100;
        Frame(panel);
        _mouse.WheelDelta = -1f;

        Frame(panel);

        Assert.AreEqual(20f, panel.ScrollOffset, delta: 0.001f);
    }

    [TestMethod]
    public void Wheel_WhenMouseNotOverPanel_DoesNotScroll()
    {
        ScrollingListPanel panel = MakeScrollable();
        _mouse.ClientX = 999;
        _mouse.ClientY = 999;
        _mouse.WheelDelta = -1f;

        Frame(panel);

        Assert.AreEqual(0f, panel.ScrollOffset);
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
