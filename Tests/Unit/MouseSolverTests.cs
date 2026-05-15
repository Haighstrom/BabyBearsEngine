using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MouseSolverTests
{
    private sealed class FakeController : IClickController
    {
        public bool PassOnMouse { get; set; } = false;
        public bool MouseIsOver { get; private set; } = false;

        public void SetMouseOver(bool isMouseOver)
        {
            MouseIsOver = isMouseOver;
        }
    }

    [TestCleanup]
    public void Cleanup() => MouseSolver.Reset();

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

    // Top-most only (PassOnMouse = false)

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

    // PassOnMouse propagation

    [TestMethod]
    public void Update_TopHasPassOnMouse_BothReceiveMouseOver()
    {
        FakeController bottom = new();
        FakeController top = new() { PassOnMouse = true };
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsTrue(bottom.MouseIsOver);
    }

    [TestMethod]
    public void Update_BottomHasPassOnMouse_TopMostOnly_OnlyTopReceivesMouseOver()
    {
        FakeController bottom = new() { PassOnMouse = true };
        FakeController top = new();
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsFalse(bottom.MouseIsOver);
    }

    [TestMethod]
    public void Update_ThreeControllers_TopTwoPassOn_AllThreeReceiveMouseOver()
    {
        FakeController bottom = new();
        FakeController middle = new() { PassOnMouse = true };
        FakeController top = new() { PassOnMouse = true };
        MouseSolver.RegisterMouseOver(bottom);
        MouseSolver.RegisterMouseOver(middle);
        MouseSolver.RegisterMouseOver(top);

        MouseSolver.Update();

        Assert.IsTrue(top.MouseIsOver);
        Assert.IsTrue(middle.MouseIsOver);
        Assert.IsTrue(bottom.MouseIsOver);
    }

    [TestMethod]
    public void Update_ThreeControllers_OnlyTopPassesOn_StopsAtMiddle()
    {
        FakeController bottom = new();
        FakeController middle = new();
        FakeController top = new() { PassOnMouse = true };
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
}
