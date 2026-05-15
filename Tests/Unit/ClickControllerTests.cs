using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "<Pending>")]
[TestClass]
public class ClickControllerTests
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
        // Mouse position is always (999, 999) so this rect never contains it —
        // prevents the controller from registering with MouseSolver during tests.
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

        _controller.MouseEntered += () => _events.Add("MouseEntered");
        _controller.MouseExited += () => _events.Add("MouseExited");
        _controller.LeftPressed += () => _events.Add("LeftPressed");
        _controller.LeftClicked += () => _events.Add("LeftClicked");
        _controller.Hovered += () => _events.Add("Hovered");
        _controller.HoverCancelled += () => _events.Add("HoverCancelled");
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

    // None state

    [TestMethod]
    public void Update_FromNone_MouseNotOver_RaisesNoEvents()
    {
        Frame(isOver: false);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_FromNone_MouseOver_RaisesMouseEntered()
    {
        Frame(isOver: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered" }, _events);
    }

    [TestMethod]
    public void Update_FromNone_MouseOverWithLeftPressed_RaisesMouseEnteredThenLeftPressed()
    {
        Frame(isOver: true, leftPressed: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered", "LeftPressed" }, _events);
    }

    // MouseOver state

    [TestMethod]
    public void Update_FromMouseOver_MouseExited_RaisesMouseExited()
    {
        Frame(isOver: true);
        _events.Clear();

        Frame(isOver: false);

        CollectionAssert.AreEqual(new[] { "MouseExited" }, _events);
    }

    [TestMethod]
    public void Update_FromMouseOver_LeftPressed_RaisesLeftPressed()
    {
        Frame(isOver: true);
        _events.Clear();

        Frame(isOver: true, leftPressed: true);

        CollectionAssert.AreEqual(new[] { "LeftPressed" }, _events);
    }

    [TestMethod]
    public void Update_FromMouseOver_HoverTimeNotReached_RaisesNoHoveredEvent()
    {
        Frame(isOver: true);
        _events.Clear();

        Frame(isOver: true, elapsed: 0.4);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_FromMouseOver_HoverTimeReached_RaisesHovered()
    {
        Frame(isOver: true);
        _events.Clear();

        Frame(isOver: true, elapsed: 0.5);

        CollectionAssert.AreEqual(new[] { "Hovered" }, _events);
    }

    // Hovering state

    [TestMethod]
    public void Update_FromHovering_MouseExited_RaisesHoverCancelledThenMouseExited()
    {
        Frame(isOver: true);
        Frame(isOver: true, elapsed: 0.5);
        _events.Clear();

        Frame(isOver: false);

        CollectionAssert.AreEqual(new[] { "HoverCancelled", "MouseExited" }, _events);
    }

    [TestMethod]
    public void Update_FromHovering_LeftPressed_RaisesHoverCancelledThenLeftPressed()
    {
        Frame(isOver: true);
        Frame(isOver: true, elapsed: 0.5);
        _events.Clear();

        Frame(isOver: true, leftPressed: true);

        CollectionAssert.AreEqual(new[] { "HoverCancelled", "LeftPressed" }, _events);
    }

    // MouseDownInside state

    [TestMethod]
    public void Update_FromMouseDownInside_LeftReleasedWhileOver_RaisesLeftClicked()
    {
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        _events.Clear();

        Frame(isOver: true, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "LeftClicked" }, _events);
    }

    [TestMethod]
    public void Update_FromMouseDownInside_MouseExitedNoRelease_RaisesNoEvents()
    {
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        _events.Clear();

        Frame(isOver: false);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_FromMouseDownInside_LeftReleasedAndMouseExited_SameFrame_RaisesMouseExited_NotLeftClicked()
    {
        // Edge case: exit and release occur on the same frame (e.g. fast mouse movement places
        // the cursor outside the region on the same frame the button comes up). Exit takes
        // priority — the interaction is cancelled: MouseExited fires, LeftClicked does not.
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        _events.Clear();

        Frame(isOver: false, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseExited" }, _events);
    }

    // MouseDownOutside state

    [TestMethod]
    public void Update_FromMouseDownOutside_LeftReleasedOutside_RaisesMouseExited()
    {
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: false);
        _events.Clear();

        Frame(isOver: false, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseExited" }, _events);
    }

    [TestMethod]
    public void Update_FromMouseDownOutside_MouseReenteredNoRelease_RaisesNoEvents()
    {
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: false);
        _events.Clear();

        Frame(isOver: true);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_FromMouseDownOutside_MouseReenteredThenLeftReleased_RaisesLeftClicked()
    {
        // Dragging outside then back inside and releasing counts as a successful click.
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: false);
        Frame(isOver: true);
        _events.Clear();

        Frame(isOver: true, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "LeftClicked" }, _events);
    }

    [TestMethod]
    public void Update_FromMouseDownOutside_MouseReenteredAndLeftReleased_SameFrame_RaisesMouseExited_NotLeftClicked()
    {
        // Edge case from issue #1: mouse over → pressed → exited → re-enters AND releases
        // on the same frame. Because the cursor was outside at some point while the button
        // was held, the interaction is treated as cancelled regardless of re-entry.
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: false);
        _events.Clear();

        Frame(isOver: true, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseExited" }, _events);
    }

    // Full event-sequence tests

    [TestMethod]
    public void SuccessfulClick_RaisesExpectedEventSequence()
    {
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: true, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered", "LeftPressed", "LeftClicked" }, _events);
    }

    [TestMethod]
    public void DragAndCancelClick_RaisesExpectedEventSequence()
    {
        Frame(isOver: true);
        Frame(isOver: true, leftPressed: true);
        Frame(isOver: false);
        Frame(isOver: false, leftReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered", "LeftPressed", "MouseExited" }, _events);
    }
}
