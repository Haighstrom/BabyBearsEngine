using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ClickControllerDoubleClickTests
{
    private sealed class FakeMouse : IMouse
    {
        public bool LeftDown { get; set; } = false;
        public bool MiddleDown { get; set; } = false;
        public bool RightDown { get; set; } = false;
        public bool LeftUp { get; set; } = false;
        public bool MiddleUp { get; set; } = false;
        public bool RightUp { get; set; } = false;
        public bool LeftPressed { get; set; } = false;
        public bool MiddlePressed { get; set; } = false;
        public bool RightPressed { get; set; } = false;
        public bool LeftReleased { get; set; } = false;
        public bool MiddleReleased { get; set; } = false;
        public bool RightReleased { get; set; } = false;
        public int ClientX { get; set; } = 999;
        public int ClientY { get; set; } = 999;
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

    private sealed class FakeTarget : IMouseInteractable
    {
        public Rect PositionOnScreen { get; } = new(0f, 0f, 10f, 10f);
    }

    private FakeMouse _mouse = null!;
    private ClickController _controller = null!;
    private List<string> _events = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new();
        EngineConfiguration.MouseService = _mouse;
        _controller = new(new FakeTarget());
        _events = [];

        _controller.LeftClicked += () => _events.Add("LeftClicked");
        _controller.LeftDoubleClicked += () => _events.Add("LeftDoubleClicked");
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    private void Frame(bool isOver, bool leftPressed = false, bool leftReleased = false, double elapsed = 0.016)
    {
        _mouse.LeftPressed = leftPressed;
        _mouse.LeftReleased = leftReleased;
        _controller.SetMouseOver(isOver);
        _controller.Update(elapsed);
    }

    private void Click()
    {
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: true, leftReleased: true);
    }

    // Single-click behaviour is unchanged

    [TestMethod]
    public void SingleClick_RaisesLeftClicked_NotLeftDoubleClicked()
    {
        Frame(isOver: true);
        Click();

        CollectionAssert.AreEqual(new[] { "LeftClicked" }, _events);
    }

    // Two clicks within the window → double-click

    [TestMethod]
    public void TwoClicksWithinWindow_SecondClickRaisesLeftDoubleClicked()
    {
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.3);
        _events.Clear();

        Click();

        Assert.Contains("LeftDoubleClicked", _events);
    }

    [TestMethod]
    public void TwoClicksWithinWindow_DefaultFlag_SecondClickRaisesBothEvents()
    {
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.3);
        _events.Clear();

        Click();

        Assert.Contains("LeftClicked", _events);
        Assert.Contains("LeftDoubleClicked", _events);
    }

    [TestMethod]
    public void TwoClicksWithinWindow_FlagFalse_SecondClickRaisesOnlyDoubleClicked()
    {
        _controller.DoubleClickTriggersSingleClick = false;
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.3);
        _events.Clear();

        Click();

        Assert.DoesNotContain("LeftClicked", _events);
        Assert.Contains("LeftDoubleClicked", _events);
    }

    // Two clicks outside the window → no double-click

    [TestMethod]
    public void TwoClicksOutsideWindow_SecondClickRaisesLeftClicked_NotLeftDoubleClicked()
    {
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.6);
        _events.Clear();

        Click();

        CollectionAssert.AreEqual(new[] { "LeftClicked" }, _events);
    }

    // Window boundary

    [TestMethod]
    public void TwoClicksExactlyAtWindowBoundary_RaisesLeftDoubleClicked()
    {
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.5);
        _events.Clear();

        // The two click frames each add a small delta; this boundary test uses elapsed=0
        // to keep the timer precisely at 0.5 when the second release fires.
        Frame(isOver: true, leftPressed: true, elapsed: 0);
        Frame(isOver: true, leftReleased: true, elapsed: 0);

        Assert.Contains("LeftDoubleClicked", _events);
    }

    // Custom window

    [TestMethod]
    public void CustomWindow_SmallWindow_ClicksThatWouldOtherwiseDoubleClick_DoNot()
    {
        _controller.DoubleClickWindow = 0.1;
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.3);
        _events.Clear();

        Click();

        Assert.DoesNotContain("LeftDoubleClicked", _events);
    }

    // Timer resets after each registered click

    [TestMethod]
    public void AfterDoubleClick_TimerResets_ThirdClickIsNotDoubleClick()
    {
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.3);
        Click();                                   // double-click
        Frame(isOver: true, elapsed: 0.6);         // exceed window from the second click
        _events.Clear();

        Click();

        CollectionAssert.AreEqual(new[] { "LeftClicked" }, _events);
    }

    [TestMethod]
    public void AfterDoubleClick_ThirdClickWithinWindow_IsAnotherDoubleClick()
    {
        Frame(isOver: true);
        Click();
        Frame(isOver: true, elapsed: 0.3);
        Click();                                   // double-click
        Frame(isOver: true, elapsed: 0.2);         // within window from the second click
        _events.Clear();

        Click();

        Assert.Contains("LeftDoubleClicked", _events);
    }
}
