using System;
using System.Drawing;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WindowFacadeTests
{
    private sealed class FakeWindow : IWindow
    {
        public WindowBorder Border { get; set; }
        public bool CursorLockedToWindow { get; set; }
        public CursorShape Cursor { get; set; }
        public bool CursorVisible { get; set; }
        public bool CloseOnXButton { get; set; }
        public int Height { get; set; }
        public WindowIcon Icon { get; set; } = new WindowIcon();
        public Point MaxClientSize { get; set; }
        public Point MinClientSize { get; set; }
        public WindowState State { get; set; }
        public string Title { get; set; } = "";
        public bool VSync { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public event Action<WindowResizeEventArgs>? Resize;

        public bool CentreCalled { get; private set; }
        public bool CloseCalled { get; private set; }

        public void Centre() => CentreCalled = true;
        public void Close() => CloseCalled = true;

        public void RaiseResize(int width, int height) => Resize?.Invoke(new WindowResizeEventArgs(width, height));
    }

    private FakeWindow _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeWindow();
        EngineConfiguration.WindowService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void Member_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Window.Width);
    }

    // Read/write properties

    [TestMethod]
    public void Border_RoundTripsThroughService()
    {
        Window.Border = WindowBorder.Fixed;
        Assert.AreEqual(WindowBorder.Fixed, _fake.Border);
        Assert.AreEqual(WindowBorder.Fixed, Window.Border);
    }

    [TestMethod]
    public void CursorLockedToWindow_RoundTripsThroughService()
    {
        Window.CursorLockedToWindow = true;
        Assert.IsTrue(_fake.CursorLockedToWindow);
        Assert.IsTrue(Window.CursorLockedToWindow);
    }

    [TestMethod]
    public void Cursor_RoundTripsThroughService()
    {
        Window.Cursor = CursorShape.Crosshair;
        Assert.AreEqual(CursorShape.Crosshair, _fake.Cursor);
        Assert.AreEqual(CursorShape.Crosshair, Window.Cursor);
    }

    [TestMethod]
    public void CursorVisible_RoundTripsThroughService()
    {
        Window.CursorVisible = true;
        Assert.IsTrue(_fake.CursorVisible);
        Assert.IsTrue(Window.CursorVisible);
    }

    [TestMethod]
    public void CloseOnXButton_RoundTripsThroughService()
    {
        Window.CloseOnXButton = true;
        Assert.IsTrue(_fake.CloseOnXButton);
        Assert.IsTrue(Window.CloseOnXButton);
    }

    [TestMethod]
    public void Icon_RoundTripsThroughService()
    {
        var icon = new WindowIcon(2, 2, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
        Window.Icon = icon;
        Assert.AreSame(icon, _fake.Icon);
        Assert.AreSame(icon, Window.Icon);
    }

    [TestMethod]
    public void MaxClientSize_RoundTripsThroughService()
    {
        Window.MaxClientSize = new Point(800, 600);
        Assert.AreEqual(new Point(800, 600), _fake.MaxClientSize);
        Assert.AreEqual(new Point(800, 600), Window.MaxClientSize);
    }

    [TestMethod]
    public void MinClientSize_RoundTripsThroughService()
    {
        Window.MinClientSize = new Point(100, 50);
        Assert.AreEqual(new Point(100, 50), _fake.MinClientSize);
        Assert.AreEqual(new Point(100, 50), Window.MinClientSize);
    }

    [TestMethod]
    public void State_RoundTripsThroughService()
    {
        Window.State = WindowState.Maximized;
        Assert.AreEqual(WindowState.Maximized, _fake.State);
        Assert.AreEqual(WindowState.Maximized, Window.State);
    }

    [TestMethod]
    public void Title_RoundTripsThroughService()
    {
        Window.Title = "test";
        Assert.AreEqual("test", _fake.Title);
        Assert.AreEqual("test", Window.Title);
    }

    [TestMethod]
    public void VSync_RoundTripsThroughService()
    {
        Window.VSync = true;
        Assert.IsTrue(_fake.VSync);
        Assert.IsTrue(Window.VSync);
    }

    [TestMethod]
    public void X_RoundTripsThroughService()
    {
        Window.X = 42;
        Assert.AreEqual(42, _fake.X);
        Assert.AreEqual(42, Window.X);
    }

    [TestMethod]
    public void Y_RoundTripsThroughService()
    {
        Window.Y = 99;
        Assert.AreEqual(99, _fake.Y);
        Assert.AreEqual(99, Window.Y);
    }

    // Read-only properties

    [TestMethod]
    public void Width_ReadsFromService()
    {
        _fake.Width = 1024;
        Assert.AreEqual(1024, Window.Width);
    }

    [TestMethod]
    public void Height_ReadsFromService()
    {
        _fake.Height = 768;
        Assert.AreEqual(768, Window.Height);
    }

    // Methods

    [TestMethod]
    public void Centre_DelegatesToService()
    {
        Window.Centre();
        Assert.IsTrue(_fake.CentreCalled);
    }

    [TestMethod]
    public void Close_DelegatesToService()
    {
        Window.Close();
        Assert.IsTrue(_fake.CloseCalled);
    }

    // Resize event subscription

    [TestMethod]
    public void Resize_SubscribingViaFacade_FiresFromService()
    {
        WindowResizeEventArgs? received = null;
        Window.Resize += args => received = args;

        _fake.RaiseResize(800, 600);

        Assert.IsNotNull(received);
        Assert.AreEqual(800, received.Width);
        Assert.AreEqual(600, received.Height);
    }

    [TestMethod]
    public void Resize_UnsubscribingViaFacade_StopsReceivingEvents()
    {
        int callCount = 0;
        Action<WindowResizeEventArgs> handler = _ => callCount++;
        Window.Resize += handler;
        _fake.RaiseResize(1, 1);
        Assert.AreEqual(1, callCount);

        Window.Resize -= handler;
        _fake.RaiseResize(2, 2);
        Assert.AreEqual(1, callCount);
    }

    // Service substitution after install

    [TestMethod]
    public void ReplacingService_RoutesToNewInstance()
    {
        var second = new FakeWindow { Width = 999 };
        EngineConfiguration.WindowService = second;
        Assert.AreEqual(999, Window.Width);
    }
}
