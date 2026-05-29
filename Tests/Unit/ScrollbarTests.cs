using System.Collections.Generic;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ScrollbarTests
{
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

    // Test root that gives the scrollbar a parent so it has a valid screen position.
    // GetWindowCoordinates returns the input unchanged — same as a World at the tree root.
    private sealed class FakeRoot : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static Scrollbar Attach(Scrollbar bar)
    {
        bar.Parent = new FakeRoot();
        return bar;
    }

    private static Scrollbar MakeH(float thumbProportion = 0.2f, float amountFilled = 0f) =>
        Attach(new(200, 20, ScrollbarDirection.Horizontal, thumbProportion, amountFilled));

    private static Scrollbar MakeV(float thumbProportion = 0.2f, float amountFilled = 0f) =>
        Attach(new(20, 200, ScrollbarDirection.Vertical, thumbProportion, amountFilled));

    // Mouse is at (5,5) by default — inside a scrollbar at origin with any reasonable size.
    private static Scrollbar MakeScrollable(ScrollbarDirection direction = ScrollbarDirection.Vertical, float amountFilled = 0.5f) =>
        Attach(new(20, 200, direction, amountFilled: amountFilled, scrollOnMouseWheel: true));

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
    private static void Frame(Scrollbar bar, double elapsed = 0.016)
    {
        bar.Update(elapsed);
        MouseSolver.Update();
    }

    // Initial state

    [TestMethod]
    public void Constructor_AmountFilledIsZeroByDefault()
    {
        Scrollbar bar = MakeH();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Constructor_InitialAmountFilled_IsApplied()
    {
        Scrollbar bar = MakeH(amountFilled: 0.5f);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Constructor_ThumbProportionIsSet()
    {
        Scrollbar bar = MakeH(thumbProportion: 0.3f);

        Assert.AreEqual(0.3f, bar.ThumbProportion);
    }

    // AmountFilled clamping

    [TestMethod]
    public void AmountFilled_SetAboveOne_ClampsToOne()
    {
        Scrollbar bar = MakeH();

        bar.AmountFilled = 1.5f;

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetBelowZero_ClampsToZero()
    {
        Scrollbar bar = MakeH();

        bar.AmountFilled = -0.5f;

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetSameValue_DoesNotFireScrollChanged()
    {
        Scrollbar bar = MakeH(amountFilled: 0.5f);
        int fired = 0;
        bar.ScrollChanged += (_, _) => fired++;

        bar.AmountFilled = 0.5f;

        Assert.AreEqual(0, fired);
    }

    // ScrollChanged event

    [TestMethod]
    public void AmountFilled_SetNewValue_RaisesScrollChanged()
    {
        Scrollbar bar = MakeH();
        bool raised = false;
        bar.ScrollChanged += (_, _) => raised = true;

        bar.AmountFilled = 0.5f;

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ScrollChanged_EventArgs_HasCorrectOldAndNewValues()
    {
        Scrollbar bar = MakeH(amountFilled: 0.2f);
        ScrollChangedEventArgs? args = null;
        bar.ScrollChanged += (_, e) => args = e;

        bar.AmountFilled = 0.8f;

        Assert.IsNotNull(args);
        Assert.AreEqual(0.2f, args.OldValue);
        Assert.AreEqual(0.8f, args.NewValue);
    }

    [TestMethod]
    public void ScrollChanged_ClampsOutOfRangeValue_ReportsClampedInArgs()
    {
        Scrollbar bar = MakeH(amountFilled: 0.5f);
        ScrollChangedEventArgs? args = null;
        bar.ScrollChanged += (_, e) => args = e;

        bar.AmountFilled = 2f;

        Assert.IsNotNull(args);
        Assert.AreEqual(1f, args.NewValue);
    }

    // ThumbProportion clamping

    [TestMethod]
    public void ThumbProportion_SetAboveOne_ClampsToOne()
    {
        Scrollbar bar = MakeH();

        bar.ThumbProportion = 1.5f;

        Assert.AreEqual(1f, bar.ThumbProportion);
    }

    [TestMethod]
    public void ThumbProportion_SetBelowMinimum_ClampsToMinimum()
    {
        Scrollbar bar = MakeH();

        bar.ThumbProportion = 0.01f;

        Assert.IsGreaterThanOrEqualTo(0.05f, bar.ThumbProportion);
    }

    // Vertical variant construction

    [TestMethod]
    public void Vertical_Constructor_AmountFilledIsZeroByDefault()
    {
        Scrollbar bar = MakeV();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Vertical_AmountFilled_SetNewValue_RaisesScrollChanged()
    {
        Scrollbar bar = MakeV();
        bool raised = false;
        bar.ScrollChanged += (_, _) => raised = true;

        bar.AmountFilled = 0.5f;

        Assert.IsTrue(raised);
    }

    // -------------------------------------------------------------------------
    // Mouse wheel scrolling

    [TestMethod]
    public void WheelScrollStep_DefaultIsPointOne()
    {
        Scrollbar bar = MakeScrollable();

        Assert.AreEqual(0.1f, bar.WheelScrollStep);
    }

    [TestMethod]
    public void ScrollUp_DecreasesAmountFilled()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.5f);
        Frame(bar); // mouse enters, mouseIsOver = true
        _mouse.WheelDelta = 1f;

        Frame(bar);

        Assert.AreEqual(0.4f, bar.AmountFilled, delta: 0.001f);
    }

    [TestMethod]
    public void ScrollDown_IncreasesAmountFilled()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.5f);
        Frame(bar);
        _mouse.WheelDelta = -1f;

        Frame(bar);

        Assert.AreEqual(0.6f, bar.AmountFilled, delta: 0.001f);
    }

    [TestMethod]
    public void Scroll_ClampsAtZero()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.05f);
        Frame(bar);
        _mouse.WheelDelta = 1f;

        Frame(bar);

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Scroll_ClampsAtOne()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.95f);
        Frame(bar);
        _mouse.WheelDelta = -1f;

        Frame(bar);

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void Scroll_WhenMouseNotOver_DoesNotFire()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.5f);
        _mouse.ClientX = 999;
        _mouse.ClientY = 999;
        _mouse.WheelDelta = 1f;

        Frame(bar);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Scroll_FiresScrollChanged()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.5f);
        bool raised = false;
        bar.ScrollChanged += (_, _) => raised = true;
        Frame(bar);
        _mouse.WheelDelta = 1f;

        Frame(bar);

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void Scroll_SetsWheelScrollConsumed()
    {
        Scrollbar bar = MakeScrollable(amountFilled: 0.5f);
        Frame(bar);
        _mouse.WheelDelta = 1f;
        bar.Update(0.016); // entity fires scroll and sets flag before MouseSolver.Update() resets it

        Assert.IsTrue(MouseSolver.WheelScrollConsumed);
    }

    [TestMethod]
    public void ScrollOnMouseWheel_False_DoesNotScroll()
    {
        Scrollbar bar = Attach(new(20, 200, ScrollbarDirection.Vertical, amountFilled: 0.5f, scrollOnMouseWheel: false));
        _mouse.WheelDelta = 1f;

        Frame(bar);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }
}
