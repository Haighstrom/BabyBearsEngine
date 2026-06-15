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
        public bool Disabled { get; set; } = false;

        // IAddable surface. Exists defaults to true so the controller treats the target as
        // tree-connected; flip it to exercise the detached-target suppression path.
        public IContainer? Parent { get; set; } = null;
        public bool Exists { get; set; } = true;
        public event EventHandler? Added;
        public event EventHandler? Removed;
        public void Remove() { }
    }

    private FakeMouse _mouse = null!;
    private FakeTarget _target = null!;
    private ClickController _controller = null!;
    private List<string> _events = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new();
        EngineConfiguration.MouseService = _mouse;
        _target = new FakeTarget();
        _controller = new(_target);
        _events = [];

        _controller.HoverCancelled += () => _events.Add("HoverCancelled");
        _controller.Hovered += () => _events.Add("Hovered");
        _controller.LeftClicked += () => _events.Add("LeftClicked");
        _controller.LeftPressed += () => _events.Add("LeftPressed");
        _controller.MouseEntered += () => _events.Add("MouseEntered");
        _controller.MouseExited += () => _events.Add("MouseExited");
        _controller.RightClicked += () => _events.Add("RightClicked");
        _controller.RightPressed += () => _events.Add("RightPressed");
    }

    [TestCleanup]
    public void Cleanup()
    {
        EngineConfiguration.Reset();
        MouseSolver.Reset();
    }

    private void Frame(bool isOver, bool leftPressed = false, bool leftReleased = false, double elapsed = 0.016)
    {
        _mouse.LeftPressed = leftPressed;
        _mouse.LeftReleased = leftReleased;
        _controller.SetMouseOver(isOver);
        _controller.Update(elapsed);
    }

    private void RightFrame(bool isOver, bool rightPressed = false, bool rightReleased = false, double elapsed = 0.016)
    {
        _mouse.RightPressed = rightPressed;
        _mouse.RightReleased = rightReleased;
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

    // Right button — None state

    [TestMethod]
    public void Update_FromNone_MouseOverWithRightPressed_RaisesMouseEnteredThenRightPressed()
    {
        RightFrame(isOver: true, rightPressed: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered", "RightPressed" }, _events);
    }

    // Right button — MouseOver state

    [TestMethod]
    public void Update_FromMouseOver_RightPressed_RaisesRightPressed()
    {
        RightFrame(isOver: true);
        _events.Clear();

        RightFrame(isOver: true, rightPressed: true);

        CollectionAssert.AreEqual(new[] { "RightPressed" }, _events);
    }

    // Right button — Hovering state
    // Note: the main state machine only transitions out of Hovering on LeftPressed,
    // so a right press while hovering does not cancel the hover or raise HoverCancelled.

    [TestMethod]
    public void Update_FromHovering_RightPressed_RaisesRightPressed()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, elapsed: 0.5);
        _events.Clear();

        RightFrame(isOver: true, rightPressed: true);

        CollectionAssert.AreEqual(new[] { "RightPressed" }, _events);
    }

    // Right button — RightDownInside state
    // Note: the main state machine stays in MouseOver while only the right button is held,
    // so cursor exit fires MouseExited immediately (it does not wait for button release).

    [TestMethod]
    public void Update_FromRightDownInside_RightReleasedWhileOver_RaisesRightClicked()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        _events.Clear();

        RightFrame(isOver: true, rightReleased: true);

        CollectionAssert.AreEqual(new[] { "RightClicked" }, _events);
    }

    [TestMethod]
    public void Update_FromRightDownInside_MouseExitedNoRelease_RaisesMouseExited()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        _events.Clear();

        RightFrame(isOver: false);

        CollectionAssert.AreEqual(new[] { "MouseExited" }, _events);
    }

    [TestMethod]
    public void Update_FromRightDownInside_RightReleasedAndMouseExited_SameFrame_RaisesMouseExited_NotRightClicked()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        _events.Clear();

        RightFrame(isOver: false, rightReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseExited" }, _events);
    }

    // Right button — RightDownOutside state

    [TestMethod]
    public void Update_FromRightDownOutside_RightReleasedOutside_RaisesNoEvents()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        RightFrame(isOver: false);
        _events.Clear();

        RightFrame(isOver: false, rightReleased: true);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_FromRightDownOutside_MouseReenteredNoRelease_RaisesMouseEntered()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        RightFrame(isOver: false);
        _events.Clear();

        RightFrame(isOver: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered" }, _events);
    }

    [TestMethod]
    public void Update_FromRightDownOutside_MouseReenteredThenRightReleased_RaisesRightClicked()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        RightFrame(isOver: false);
        RightFrame(isOver: true);
        _events.Clear();

        RightFrame(isOver: true, rightReleased: true);

        CollectionAssert.AreEqual(new[] { "RightClicked" }, _events);
    }

    [TestMethod]
    public void Update_FromRightDownOutside_MouseReenteredAndRightReleased_SameFrame_RaisesMouseEntered_NotRightClicked()
    {
        // Release takes priority over re-entry in the RightDownOutside state, so no RightClicked.
        // The main state machine sees the re-entry from None and fires MouseEntered.
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        RightFrame(isOver: false);
        _events.Clear();

        RightFrame(isOver: true, rightReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered" }, _events);
    }

    // Right button — full event-sequence tests

    [TestMethod]
    public void SuccessfulRightClick_RaisesExpectedEventSequence()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        RightFrame(isOver: true, rightReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered", "RightPressed", "RightClicked" }, _events);
    }

    [TestMethod]
    public void DragAndCancelRightClick_RaisesExpectedEventSequence()
    {
        RightFrame(isOver: true);
        RightFrame(isOver: true, rightPressed: true);
        RightFrame(isOver: false);
        RightFrame(isOver: false, rightReleased: true);

        CollectionAssert.AreEqual(new[] { "MouseEntered", "RightPressed", "MouseExited" }, _events);
    }

    // -------------------------------------------------------------------------
    // InterceptsMouseScroll

    [TestMethod]
    public void InterceptsMouseScroll_DefaultIsFalse()
    {
        Assert.IsFalse(_controller.InterceptsMouseScroll);
    }

    [TestMethod]
    public void ScrollWheelMoved_WhenIntercepting_MouseOver_WheelMoved_Fires()
    {
        _controller.InterceptsMouseScroll = true;
        float? received = null;
        _controller.ScrollWheelMoved += d => received = d;

        Frame(isOver: true);
        _mouse.WheelDelta = 3f;
        Frame(isOver: true);

        Assert.AreEqual(3f, received);
    }

    [TestMethod]
    public void ScrollWheelMoved_WhenNotIntercepting_DoesNotFire()
    {
        float? received = null;
        _controller.ScrollWheelMoved += d => received = d;

        Frame(isOver: true);
        _mouse.WheelDelta = 3f;
        Frame(isOver: true);

        Assert.IsNull(received);
    }

    [TestMethod]
    public void ScrollWheelMoved_WhenIntercepting_MouseNotOver_DoesNotFire()
    {
        _controller.InterceptsMouseScroll = true;
        float? received = null;
        _controller.ScrollWheelMoved += d => received = d;

        _mouse.WheelDelta = 3f;
        Frame(isOver: false);

        Assert.IsNull(received);
    }

    [TestMethod]
    public void ScrollWheelMoved_WheelDeltaZero_DoesNotFire()
    {
        _controller.InterceptsMouseScroll = true;
        float? received = null;
        _controller.ScrollWheelMoved += d => received = d;

        Frame(isOver: true);
        _mouse.WheelDelta = 0f;
        Frame(isOver: true);

        Assert.IsNull(received);
    }

    [TestMethod]
    public void WheelScrollConsumed_SetWhenScrollIntercepted()
    {
        _controller.InterceptsMouseScroll = true;

        Frame(isOver: true);
        _mouse.WheelDelta = 1f;
        Frame(isOver: true);

        Assert.IsTrue(MouseSolver.WheelScrollConsumed);
    }

    [TestMethod]
    public void WheelScrollConsumed_NotSetWhenNotIntercepting()
    {
        Frame(isOver: true);
        _mouse.WheelDelta = 1f;
        Frame(isOver: true);

        Assert.IsFalse(MouseSolver.WheelScrollConsumed);
    }

    // Cancellation on disable / detach

    [TestMethod]
    public void Update_DisabledMidPress_FiresMouseExitedSoButtonDoesntStickPressed()
    {
        // Enter then press to land in MouseDownInside.
        Frame(isOver: true, leftPressed: true);
        _events.Clear();

        // Target is disabled mid-press — controller should signal cancellation rather than
        // silently zero the state, otherwise subscribers that latched on LeftPressed without
        // a matching MouseExited would stay "highlighted" forever.
        _target.Disabled = true;
        Frame(isOver: true);

        Assert.Contains("MouseExited", _events);
    }

    [TestMethod]
    public void Update_DisabledMidHover_FiresHoverCancelledAndMouseExited()
    {
        // Enter and hold past the hover delay (default 0.5s) to land in Hovering.
        Frame(isOver: true);
        Frame(isOver: true, elapsed: 1.0);
        _events.Clear();

        _target.Disabled = true;
        Frame(isOver: true);

        Assert.Contains("HoverCancelled", _events);
        Assert.Contains("MouseExited", _events);
    }

    [TestMethod]
    public void Update_DisabledWhileJustOver_FiresMouseExited()
    {
        // Land in MouseOver (entered but not hovering yet, not pressed).
        Frame(isOver: true);
        _events.Clear();

        _target.Disabled = true;
        Frame(isOver: true);

        Assert.Contains("MouseExited", _events);
    }

    [TestMethod]
    public void Update_DisabledFromNoneState_RaisesNoEvents()
    {
        // Never entered — no state to cancel.
        _target.Disabled = true;
        Frame(isOver: false);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_DetachedMidPress_FiresMouseExitedSoButtonDoesntStickPressed()
    {
        // Enter then press to land in MouseDownInside.
        Frame(isOver: true, leftPressed: true);
        _events.Clear();

        // Target detaches from the tree mid-press (Exists becomes false) — treated the same as
        // disabled: signal cancellation rather than leaving subscribers stuck "pressed".
        _target.Exists = false;
        Frame(isOver: true);

        Assert.Contains("MouseExited", _events);
    }

    [TestMethod]
    public void Update_DetachedWhileJustOver_FiresMouseExited()
    {
        // Land in MouseOver (entered but not hovering yet, not pressed).
        Frame(isOver: true);
        _events.Clear();

        _target.Exists = false;
        Frame(isOver: true);

        Assert.Contains("MouseExited", _events);
    }

    [TestMethod]
    public void Update_DetachedFromNoneState_RaisesNoEvents()
    {
        // Never entered — no state to cancel.
        _target.Exists = false;
        Frame(isOver: false);

        Assert.IsEmpty(_events);
    }
}
