using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CameraRightMouseDragPanControllerTests
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
        public bool MouseIntersecting { get; set; } = true;
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

    private FakeMouse _mouse = null!;
    private FakeCamera _camera = null!;
    private CameraRightMouseDragPanController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new FakeMouse();
        _camera = new FakeCamera();
        EngineConfiguration.MouseService = _mouse;
        _controller = new CameraRightMouseDragPanController(_camera);
    }

    [TestCleanup]
    public void Cleanup()
    {
        EngineConfiguration.Reset();
    }

    private void StartDrag()
    {
        _mouse.RightPressed = true;
        _mouse.RightDown = true;
        _camera.MouseIntersecting = true;
        _controller.Update(0.016);
        _mouse.RightPressed = false;
    }

    [TestMethod]
    public void Update_RightPressedWhileIntersecting_BeginsDrag()
    {
        _camera.View.X = 50f;
        StartDrag();

        _mouse.XDelta = 10;
        _controller.Update(0.016);

        Assert.IsGreaterThan(50f, _camera.View.X); // view moved
    }

    [TestMethod]
    public void Update_RightPressedNotIntersecting_DoesNotBeginDrag()
    {
        _camera.View.X = 50f;
        _camera.MouseIntersecting = false;
        _mouse.RightPressed = true;
        _mouse.RightDown = true;
        _controller.Update(0.016);
        _mouse.RightPressed = false;

        _mouse.XDelta = 10;
        _controller.Update(0.016);

        Assert.AreEqual(50f, _camera.View.X);
    }

    [TestMethod]
    public void Update_WhileDragging_MovesViewXByDeltaDividedByTileWidth()
    {
        _camera.View.X = 50f;
        ((FakeCameraView)_camera.View).TileWidth = 2f;
        StartDrag();

        _mouse.RightDown = true;
        _mouse.XDelta = 20;
        _controller.Update(0.016);

        Assert.AreEqual(60f, _camera.View.X, delta: 0.001f); // 50 + 20/2
    }

    [TestMethod]
    public void Update_WhileDragging_MovesViewYByDeltaDividedByTileHeight()
    {
        _camera.View.Y = 50f;
        ((FakeCameraView)_camera.View).TileHeight = 2f;
        StartDrag();

        _mouse.RightDown = true;
        _mouse.YDelta = 20;
        _controller.Update(0.016);

        Assert.AreEqual(60f, _camera.View.Y, delta: 0.001f); // 50 + 20/2
    }

    [TestMethod]
    public void Update_RightUp_StopsDrag()
    {
        StartDrag();

        _mouse.RightUp = true;
        _mouse.RightDown = false;
        _controller.Update(0.016);
        _mouse.RightUp = false;

        _camera.View.X = 50f;
        _mouse.RightDown = true;
        _mouse.XDelta = 10;
        _controller.Update(0.016);

        Assert.AreEqual(50f, _camera.View.X); // drag stopped, view unchanged
    }

    [TestMethod]
    public void Update_WhileDragging_ClampsViewXToMaxXMinusViewWidth()
    {
        _camera.MaxX = 1000f;
        ((FakeCameraView)_camera.View).ViewWidth = 100f;
        _camera.View.X = 895f;
        StartDrag();

        _mouse.RightDown = true;
        _mouse.XDelta = 100;
        _controller.Update(0.016);

        Assert.AreEqual(900f, _camera.View.X, delta: 0.001f);
    }

    [TestMethod]
    public void Update_WhileDragging_ClampsViewXToMinX()
    {
        _camera.MinX = 0f;
        _camera.View.X = 5f;
        StartDrag();

        _mouse.RightDown = true;
        _mouse.XDelta = -100;
        _controller.Update(0.016);

        Assert.AreEqual(0f, _camera.View.X, delta: 0.001f);
    }

    [TestMethod]
    public void Update_ViewWiderThanWorld_XNotClamped()
    {
        _camera.MinX = 0f;
        _camera.MaxX = 50f;
        ((FakeCameraView)_camera.View).ViewWidth = 100f; // wider than world
        _camera.View.X = 0f;
        StartDrag();

        _mouse.RightDown = true;
        _mouse.XDelta = 10;
        _controller.Update(0.016);

        Assert.AreEqual(0f, _camera.View.X); // MinX >= MaxX - ViewWidth so no clamping branch runs
    }
}
