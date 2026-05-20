using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CameraMoveWhenMouseAtEdgeOrKeyPressedControllerTests
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

    private sealed class FakeKeyboard : IKeyboard
    {
        public HashSet<Keys> DownKeys { get; } = [];

        public bool KeyDown(Keys key) => DownKeys.Contains(key);
        public bool KeyPressed(Keys key) => false;
        public bool KeyReleased(Keys key) => false;
        public bool AnyKeyDown(IEnumerable<Keys> keys) => keys.Any(DownKeys.Contains);
        public bool AnyKeyDown(params Keys[] keys) => keys.Any(DownKeys.Contains);
        public bool AnyKeyPressed(IEnumerable<Keys> keys) => false;
        public bool AnyKeyPressed(params Keys[] keys) => false;
        public bool AnyKeyReleased(IEnumerable<Keys> keys) => false;
        public bool AnyKeyReleased(params Keys[] keys) => false;
        public bool AllKeysDown(IEnumerable<Keys> keys) => keys.All(DownKeys.Contains);
        public bool AllKeysDown(params Keys[] keys) => keys.All(DownKeys.Contains);
        public bool AllKeysPressed(IEnumerable<Keys> keys) => false;
        public bool AllKeysPressed(params Keys[] keys) => false;
        public bool AllKeysReleased(IEnumerable<Keys> keys) => false;
        public bool AllKeysReleased(params Keys[] keys) => false;
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

    private static readonly Keys[] s_upKeys = [Keys.W];
    private static readonly Keys[] s_downKeys = [Keys.S];
    private static readonly Keys[] s_leftKeys = [Keys.A];
    private static readonly Keys[] s_rightKeys = [Keys.D];

    private FakeMouse _mouse = null!;
    private FakeKeyboard _keyboard = null!;
    private FakeWindow _window = null!;
    private FakeCamera _camera = null!;
    private CameraMoveWhenMouseAtEdgeOrKeyPressedController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new FakeMouse();
        _keyboard = new FakeKeyboard();
        _window = new FakeWindow();
        _camera = new FakeCamera();
        EngineConfiguration.MouseService = _mouse;
        EngineConfiguration.KeyboardService = _keyboard;
        EngineConfiguration.WindowService = _window;
        _controller = new CameraMoveWhenMouseAtEdgeOrKeyPressedController(
            _camera,
            cameraMoveSpeed: 100f,
            windowEdgeDistance: 20,
            upKeys: s_upKeys,
            downKeys: s_downKeys,
            leftKeys: s_leftKeys,
            rightKeys: s_rightKeys);
    }

    [TestCleanup]
    public void Cleanup()
    {
        EngineConfiguration.Reset();
    }

    [TestMethod]
    public void Update_RightKeyDown_IncreasesViewX()
    {
        _camera.View.X = 50f;
        _keyboard.DownKeys.Add(Keys.D);

        _controller.Update(1.0);

        Assert.IsGreaterThan(50f, _camera.View.X);
    }

    [TestMethod]
    public void Update_RightKeyDown_ClampsToMaxXMinusViewWidth()
    {
        _camera.MaxX = 1000f;
        ((FakeCameraView)_camera.View).ViewWidth = 100f;
        _camera.View.X = 895f;
        _keyboard.DownKeys.Add(Keys.D);

        _controller.Update(1.0);

        Assert.AreEqual(900f, _camera.View.X, delta: 0.001f);
    }

    [TestMethod]
    public void Update_LeftKeyDown_DecreasesViewX()
    {
        _camera.View.X = 50f;
        _keyboard.DownKeys.Add(Keys.A);

        _controller.Update(1.0);

        Assert.IsLessThan(50f, _camera.View.X);
    }

    [TestMethod]
    public void Update_LeftKeyDown_ClampsToMinX()
    {
        _camera.MinX = 0f;
        _camera.View.X = 0f;
        _keyboard.DownKeys.Add(Keys.A);

        _controller.Update(1.0);

        Assert.AreEqual(0f, _camera.View.X);
    }

    [TestMethod]
    public void Update_UpKeyDown_DecreasesViewY()
    {
        _camera.View.Y = 50f;
        _keyboard.DownKeys.Add(Keys.W);

        _controller.Update(1.0);

        Assert.IsLessThan(50f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_UpKeyDown_ClampsToMinY()
    {
        _camera.MinY = 0f;
        _camera.View.Y = 0f;
        _keyboard.DownKeys.Add(Keys.W);

        _controller.Update(1.0);

        Assert.AreEqual(0f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_DownKeyDown_IncreasesViewY()
    {
        _camera.View.Y = 50f;
        _keyboard.DownKeys.Add(Keys.S);

        _controller.Update(1.0);

        Assert.IsGreaterThan(50f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_DownKeyDown_ClampsToMaxYMinusViewHeight()
    {
        _camera.MaxY = 1000f;
        ((FakeCameraView)_camera.View).ViewHeight = 100f;
        _camera.View.Y = 895f;
        _keyboard.DownKeys.Add(Keys.S);

        _controller.Update(1.0);

        Assert.AreEqual(900f, _camera.View.Y, delta: 0.001f);
    }

    [TestMethod]
    public void Update_ViewWiderThanWorld_XCentred()
    {
        _camera.MinX = 0f;
        _camera.MaxX = 50f;
        ((FakeCameraView)_camera.View).ViewWidth = 100f; // wider than world
        _camera.View.X = 999f; // any value

        _controller.Update(1.0);

        Assert.AreEqual(-25f, _camera.View.X, delta: 0.001f); // -(100 - 50) / 2
    }

    [TestMethod]
    public void Update_ViewTallerThanWorld_YCentred()
    {
        _camera.MinY = 0f;
        _camera.MaxY = 50f;
        ((FakeCameraView)_camera.View).ViewHeight = 100f; // taller than world
        _camera.View.Y = 999f; // any value

        _controller.Update(1.0);

        Assert.AreEqual(-25f, _camera.View.Y, delta: 0.001f); // -(100 - 50) / 2
    }

    [TestMethod]
    public void Update_NoKeyNoEdge_ViewDoesNotMove()
    {
        _camera.View.X = 50f;
        _camera.View.Y = 50f;
        _mouse.ClientX = 400;
        _mouse.ClientY = 300;

        _controller.Update(1.0);

        Assert.AreEqual(50f, _camera.View.X);
        Assert.AreEqual(50f, _camera.View.Y);
    }

    [TestMethod]
    public void Update_ScrollAmountProportionalToElapsed()
    {
        _camera.View.X = 50f;
        _keyboard.DownKeys.Add(Keys.D);

        _controller.Update(0.5);

        Assert.AreEqual(100f, _camera.View.X, delta: 0.001f); // 50 + 100 * 0.5
    }
}
