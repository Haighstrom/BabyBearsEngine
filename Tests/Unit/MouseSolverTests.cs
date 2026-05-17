using System.Collections.Generic;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MouseSolverTests
{
    private sealed class FakeController : IClickController
    {
        public bool ClickThrough { get; set; } = false;
        public bool MouseIsOver { get; private set; } = false;

        public void SetMouseOver(bool isMouseOver)
        {
            MouseIsOver = isMouseOver;
        }
    }

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
        public int ClientX { get; set; } = 0;
        public int ClientY { get; set; } = 0;
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

    private FakeMouse _mouse = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new();
        EngineConfiguration.MouseService = _mouse;
    }

    [TestCleanup]
    public void Cleanup()
    {
        MouseSolver.Reset();
        EngineConfiguration.Reset();
    }

    // Single controller

    [TestMethod]
    public void Update_SingleController_ReceivesMouseOver()
    {
        FakeController controller = new();
        MouseSolver.RegisterMouseOver(controller);

        MouseSolver.Update();

        Assert.IsTrue(controller.MouseIsOver);
    }

    [TestMethod]
    public void Update_NoControllers_NoExceptionThrown()
    {
        MouseSolver.Update();
    }

    // Top-most only (ClickThrough = false)

    [TestMethod]
    public void Update_TwoControllers_TopMostOnly_OnlyTopReceivesMouseOver()
    {
        FakeController bottom = new();
        FakeController top = new();
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsFalse(bottom.MouseIsOver);
    }

    // ClickThrough propagation

    [TestMethod]
    public void Update_TopHasClickThrough_BothReceiveMouseOver()
    {
        FakeController bottom = new();
        FakeController top = new() { ClickThrough = true };
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsTrue(bottom.MouseIsOver);
    }

    [TestMethod]
    public void Update_BottomHasClickThrough_TopMostOnly_OnlyTopReceivesMouseOver()
    {
        FakeController bottom = new() { ClickThrough = true };
        FakeController top = new();
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsFalse(bottom.MouseIsOver);
    }

    [TestMethod]
    public void Update_ThreeControllers_TopTwoClickThrough_AllThreeReceiveMouseOver()
    {
        FakeController bottom = new();
        FakeController middle = new() { ClickThrough = true };
        FakeController top = new() { ClickThrough = true };
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(middle);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsTrue(middle.MouseIsOver);
        Assert.IsTrue(bottom.MouseIsOver);
    }

    [TestMethod]
    public void Update_ThreeControllers_OnlyTopClicksThrough_StopsAtMiddle()
    {
        FakeController bottom = new();
        FakeController middle = new();
        FakeController top = new() { ClickThrough = true };
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(middle);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsTrue(middle.MouseIsOver);
        Assert.IsFalse(bottom.MouseIsOver);
    }

    // Previous-frame cleanup

    [TestMethod]
    public void Update_ControllerNotRegisteredThisFrame_MouseOverClearedFromPreviousFrame()
    {
        FakeController prev = new();
        FakeController current = new();

        MouseSolver.RegisterMouseOver(prev);
        MouseSolver.Update();

        MouseSolver.RegisterMouseOver(current);
        MouseSolver.Update();

        Assert.IsFalse(prev.MouseIsOver);
        Assert.IsTrue(current.MouseIsOver);
    }

    // Locking behavior (left button held)

    [TestMethod]
    public void Update_WhileLeftHeld_LockedControllerStillReceivesMouseOver()
    {
        FakeController locked = new();

        _mouse.LeftPressed = true;
        _mouse.LeftDown = true;
        MouseSolver.RegisterMouseOver(locked);
        MouseSolver.Update();

        _mouse.LeftPressed = false;
        MouseSolver.RegisterMouseOver(locked);
        MouseSolver.Update();

        Assert.IsTrue(locked.MouseIsOver);
    }

    [TestMethod]
    public void Update_WhileLeftHeld_ControllerNotInLockedSetIsSuppressed()
    {
        FakeController locked = new();
        FakeController unlocked = new();

        // Frame 1: press while only 'locked' is under cursor.
        _mouse.LeftPressed = true;
        _mouse.LeftDown = true;
        MouseSolver.RegisterMouseOver(locked);
        MouseSolver.Update();

        // Frame 2: cursor now over both, but button still held.
        _mouse.LeftPressed = false;
        MouseSolver.RegisterMouseOver(locked);
        MouseSolver.RegisterMouseOver(unlocked);
        MouseSolver.Update();

        Assert.IsTrue(locked.MouseIsOver);
        Assert.IsFalse(unlocked.MouseIsOver);
    }

    [TestMethod]
    public void Update_AfterLeftReleased_PreviouslySuppressedControllerCanReceiveMouseOver()
    {
        FakeController first = new();
        FakeController second = new();

        // Frame 1: press over 'first'.
        _mouse.LeftPressed = true;
        _mouse.LeftDown = true;
        MouseSolver.RegisterMouseOver(first);
        MouseSolver.Update();

        // Frame 2: button released; 'second' is now the only controller under cursor.
        _mouse.LeftPressed = false;
        _mouse.LeftDown = false;
        MouseSolver.RegisterMouseOver(second);
        MouseSolver.Update();

        Assert.IsFalse(first.MouseIsOver);
        Assert.IsTrue(second.MouseIsOver);
    }

    [TestMethod]
    public void Update_PressOnEmptySpace_DoesNotSuppressSubsequentHovers()
    {
        FakeController controller = new();

        // Frame 1: press with nothing under cursor — locked set will be empty.
        _mouse.LeftPressed = true;
        _mouse.LeftDown = true;
        MouseSolver.Update();

        // Frame 2: button still held, controller now under cursor.
        _mouse.LeftPressed = false;
        MouseSolver.RegisterMouseOver(controller);
        MouseSolver.Update();

        // Empty locked set means no lock is active, so the controller is not suppressed.
        Assert.IsTrue(controller.MouseIsOver);
    }
}
