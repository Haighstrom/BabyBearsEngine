using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MenuGroupTests
{
    private sealed class FakeMenu : AddableBase, IMenu
    {
        public bool IsOpen { get; private set; } = false;
        public int OpenCalls { get; private set; } = 0;
        public int CloseCalls { get; private set; } = 0;

        public void Open()  { IsOpen = true;  OpenCalls++; }
        public void Close() { IsOpen = false; CloseCalls++; }
    }

    // Open

    [TestMethod]
    public void Open_OpensTargetMenu()
    {
        MenuGroup group = new();
        FakeMenu menu = new();
        group.Register(menu);

        group.Open(menu);

        Assert.IsTrue(menu.IsOpen);
    }

    [TestMethod]
    public void Open_ClosesOtherMenus()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        FakeMenu b = new();
        FakeMenu c = new();
        group.Register(a, b, c);
        a.Open();
        b.Open();

        group.Open(c);

        Assert.IsFalse(a.IsOpen);
        Assert.IsFalse(b.IsOpen);
        Assert.IsTrue(c.IsOpen);
    }

    [TestMethod]
    public void Open_WhenTargetAlreadyOpen_RemainsOpen()
    {
        MenuGroup group = new();
        FakeMenu menu = new();
        group.Register(menu);
        menu.Open();

        group.Open(menu);

        Assert.IsTrue(menu.IsOpen);
    }

    // Toggle

    [TestMethod]
    public void Toggle_WhenClosed_OpensMenu()
    {
        MenuGroup group = new();
        FakeMenu menu = new();
        group.Register(menu);

        group.Toggle(menu);

        Assert.IsTrue(menu.IsOpen);
    }

    [TestMethod]
    public void Toggle_WhenOpen_ClosesMenu()
    {
        MenuGroup group = new();
        FakeMenu menu = new();
        group.Register(menu);
        menu.Open();

        group.Toggle(menu);

        Assert.IsFalse(menu.IsOpen);
    }

    [TestMethod]
    public void Toggle_OpensTarget_ClosesOthers()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        FakeMenu b = new();
        group.Register(a, b);
        a.Open();

        group.Toggle(b);

        Assert.IsFalse(a.IsOpen);
        Assert.IsTrue(b.IsOpen);
    }

    // CloseAll

    [TestMethod]
    public void CloseAll_ClosesEveryMenu()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        FakeMenu b = new();
        group.Register(a, b);
        a.Open();
        b.Open();

        group.CloseAll();

        Assert.IsFalse(a.IsOpen);
        Assert.IsFalse(b.IsOpen);
    }

    [TestMethod]
    public void CloseAll_WhenNoneOpen_IsNoOp()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        FakeMenu b = new();
        group.Register(a, b);

        group.CloseAll();

        Assert.AreEqual(1, a.CloseCalls);
        Assert.AreEqual(1, b.CloseCalls);
    }

    // Deduplication

    [TestMethod]
    public void Register_SameMenuTwiceInSeparateCalls_OpeningAnotherClosesItOnce()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        FakeMenu b = new();
        group.Register(a);
        group.Register(a); // duplicate registration
        group.Register(b);

        group.Open(b);

        Assert.AreEqual(1, a.CloseCalls);
    }

    [TestMethod]
    public void Register_SameMenuTwiceInOneCall_OpeningAnotherClosesItOnce()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        FakeMenu b = new();
        group.Register(a, a, b); // duplicate within the params array

        group.Open(b);

        Assert.AreEqual(1, a.CloseCalls);
    }

    [TestMethod]
    public void Register_SameMenuTwice_CloseAllCallsCloseOnce()
    {
        MenuGroup group = new();
        FakeMenu a = new();
        group.Register(a, a);

        group.CloseAll();

        Assert.AreEqual(1, a.CloseCalls);
    }
}
