using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CameraMoveWhenMouseAtEdgeControllerTests
{
    private sealed class FakeCameraView : ICameraView
    {
        public float TileHeight { get; set; } = 1f;
        public float TileWidth { get; set; } = 1f;
        public float ViewHeight { get; set; } = 100f;
        public float ViewWidth { get; set; } = 100f;
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public event EventHandler? ViewChanged;
        public (float x, float y) WorldToLocal(float worldX, float worldY) => (worldX, worldY);
    }

    private sealed class FakeCamera : ICamera
    {
        private readonly FakeCameraView _view = new();

        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Width { get; set; } = 800f;
        public float Height { get; set; } = 600f;
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public IContainer? Parent { get; set; } = null;
        public bool Exists => Parent is not null;
        public Colour BackgroundColour { get; set; } = Colour.White;
        public float GameSpeed { get; set; } = 1f;
        public float MaxX { get; set; } = 1000f;
        public float MaxY { get; set; } = 1000f;
        public float MinX { get; set; } = 0f;
        public float MinY { get; set; } = 0f;
        public bool MouseIntersecting { get; set; } = false;
        public MsaaSamples MSAASamples { get; set; } = MsaaSamples.Disabled;
        public ICameraView View => _view;

        public event EventHandler? ViewChanged;

        public event EventHandler? Added
        {
            add { }
            remove { }
        }

        public event EventHandler? Removed
        {
            add { }
            remove { }
        }

        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void Remove() { }
        public void RemoveAll() { }
        public void Update(double elapsed) { }
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
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
        public int ClientX { get; set; } = 400;
        public int ClientY { get; set; } = 300;
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

    private sealed class FakeWindow : IWindow
    {
        public WindowBorder Border { get; set; }
        public bool CursorLockedToWindow { get; set; } = false;
        public CursorShape Cursor { get; set; }
        public bool CursorVisible { get; set; } = true;
        public bool CloseOnXButton { get; set; } = true;
        public int Height { get; set; } = 600;
        public WindowIcon Icon { get; set; } = null!;
        public System.Drawing.Point MaxClientSize { get; set; }
        public System.Drawing.Point MinClientSize { get; set; }
        public WindowState State { get; set; }
        public string Title { get; set; } = "";
        public bool VSync { get; set; } = false;
        public int Width { get; set; } = 800;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public event Action<WindowResizeEventArgs>? Resize;
        public void Centre() { }
        public void Close() { }
    }

    private FakeMouse _mouse = null!;
    private FakeWindow _window = null!;
    private FakeCamera _camera = null!;
    private CameraMoveWhenMouseAtEdgeController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new FakeMouse();
        _window = new FakeWindow();
        _camera = new FakeCamera();
        EngineConfiguration.MouseService = _mouse;
        EngineConfiguration.WindowService = _window;
        _controller = new CameraMoveWhenMouseAtEdgeController(_camera, cameraMoveSpeed: 100f, windowEdgeDistance: 20);
    }

    [TestCleanup]
    public void Cleanup()
    {
        EngineConfiguration.Reset();
    }

    [TestMethod]
    public void Update_MouseInCenter_ViewDoesNotMove()
    {
        _mouse.ClientX = 400;
        _mouse.ClientY = 300;
        _camera.View.X = 50f;
        _camera.View.Y = 50f;

        _controller.Update(1.0);

        Assert.AreEqual(50f, _camera.View.X);
        Assert.AreEqual(50f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_MouseNearTopEdge_DecreasesViewY()
    {
        _mouse.ClientX = 400;
        _mouse.ClientY = 10; // < windowEdgeDistance (20)
        _camera.View.Y = 50f;

        _controller.Update(1.0);

        Assert.IsLessThan(50f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_MouseNearTopEdge_ClampsToMinY()
    {
        _mouse.ClientX = 400;
        _mouse.ClientY = 0;
        _camera.View.Y = 0f;
        _camera.MinY = 0f;

        _controller.Update(1.0);

        Assert.AreEqual(0f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_MouseNearRightEdge_IncreasesViewX()
    {
        _mouse.ClientX = 790; // > 800 - 20
        _mouse.ClientY = 300;
        _camera.View.X = 50f;

        _controller.Update(1.0);

        Assert.IsGreaterThan(50f, _camera.View.X);
    }

    [TestMethod]
    public void Update_MouseNearRightEdge_ClampsToMaxXMinusViewWidth()
    {
        _mouse.ClientX = 799;
        _mouse.ClientY = 300;
        _camera.View.X = 895f;
        _camera.MaxX = 1000f;
        ((FakeCameraView)_camera.View).ViewWidth = 100f;

        _controller.Update(1.0);

        Assert.AreEqual(900f, _camera.View.X); // MaxX - ViewWidth = 1000 - 100
    }

    [TestMethod]
    public void Update_MouseNearBottomEdge_IncreasesViewY()
    {
        _mouse.ClientX = 400;
        _mouse.ClientY = 590; // > 600 - 20
        _camera.View.Y = 50f;

        _controller.Update(1.0);

        Assert.IsGreaterThan(50f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_MouseNearBottomEdge_ClampsToMaxYMinusViewHeight()
    {
        _mouse.ClientX = 400;
        _mouse.ClientY = 599;
        _camera.View.Y = 895f;
        _camera.MaxY = 1000f;
        ((FakeCameraView)_camera.View).ViewHeight = 100f;

        _controller.Update(1.0);

        Assert.AreEqual(900f, _camera.View.Y); // MaxY - ViewHeight = 1000 - 100
    }

    [TestMethod]
    public void Update_MouseNearLeftEdge_DecreasesViewX()
    {
        _mouse.ClientX = 10; // < windowEdgeDistance (20)
        _mouse.ClientY = 300;
        _camera.View.X = 50f;

        _controller.Update(1.0);

        Assert.IsLessThan(50f, _camera.View.X);
    }

    [TestMethod]
    public void Update_MouseNearLeftEdge_ClampsToMinX()
    {
        _mouse.ClientX = 0;
        _mouse.ClientY = 300;
        _camera.View.X = 0f;
        _camera.MinX = 0f;

        _controller.Update(1.0);

        Assert.AreEqual(0f, _camera.View.X);
    }

    [TestMethod]
    public void Update_ScrollAmountProportionalToElapsed()
    {
        _mouse.ClientX = 400;
        _mouse.ClientY = 0; // near top edge
        _camera.View.Y = 200f;

        _controller.Update(0.5);

        Assert.AreEqual(150f, _camera.View.Y, delta: 0.001f); // 200 - 100 * 0.5
    }
}
