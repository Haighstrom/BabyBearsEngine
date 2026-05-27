using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CameraMouseWorldPositionTests
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
        public (float x, float y) WorldToLocal(float worldX, float worldY) => ((worldX - X) * TileWidth, (worldY - Y) * TileHeight);
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

        public (float x, float y) GetWindowCoordinates(float x, float y)
        {
            var (lx, ly) = _view.WorldToLocal(x, y);
            return (X + lx, Y + ly);
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
    private ICamera _camera = null!;

    [TestInitialize]
    public void Setup()
    {
        _mouse = new FakeMouse();
        _camera = new FakeCamera();
        EngineConfiguration.MouseService = _mouse;
    }

    [TestCleanup]
    public void Cleanup()
    {
        EngineConfiguration.Reset();
    }

    [TestMethod]
    public void MouseWorldPosition_MouseAtCameraOrigin_NoScroll_NoZoom_ReturnsZero()
    {
        _mouse.ClientX = 0;
        _mouse.ClientY = 0;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(0f, world.X);
        Assert.AreEqual(0f, world.Y);
    }

    [TestMethod]
    public void MouseWorldPosition_MouseOffsetFromOrigin_NoScroll_NoZoom_ReturnsPixelOffset()
    {
        _mouse.ClientX = 50;
        _mouse.ClientY = 30;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(50f, world.X);
        Assert.AreEqual(30f, world.Y);
    }

    [TestMethod]
    public void MouseWorldPosition_WithTileScale_DividesByTileSize()
    {
        var view = (FakeCameraView)_camera.View;
        view.TileWidth = 2f;
        view.TileHeight = 4f;
        _mouse.ClientX = 50;
        _mouse.ClientY = 40;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(25f, world.X);
        Assert.AreEqual(10f, world.Y);
    }

    [TestMethod]
    public void MouseWorldPosition_WithViewScroll_AddsScrollOffset()
    {
        _camera.View.X = 10f;
        _camera.View.Y = 5f;
        _mouse.ClientX = 0;
        _mouse.ClientY = 0;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(10f, world.X);
        Assert.AreEqual(5f, world.Y);
    }

    [TestMethod]
    public void MouseWorldPosition_WithViewScrollAndTileScale_CombinesBoth()
    {
        var view = (FakeCameraView)_camera.View;
        view.TileWidth = 2f;
        view.TileHeight = 2f;
        view.X = 10f;
        view.Y = 5f;
        _mouse.ClientX = 20;
        _mouse.ClientY = 10;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(20f, world.X, delta: 0.001f); // 10 + 20/2
        Assert.AreEqual(10f, world.Y, delta: 0.001f); // 5 + 10/2
    }

    [TestMethod]
    public void MouseWorldPosition_WithCameraOffsetWithinParent_SubtractsCameraPosition()
    {
        _camera.X = 100f;
        _camera.Y = 50f;
        _mouse.ClientX = 100;
        _mouse.ClientY = 50;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(0f, world.X);
        Assert.AreEqual(0f, world.Y);
    }

    [TestMethod]
    public void MouseWorldPosition_MouseOutsideCamera_ReturnsExtrapolatedWorldPoint()
    {
        _camera.X = 100f;
        _camera.Y = 100f;
        _mouse.ClientX = 50;
        _mouse.ClientY = 75;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(-50f, world.X);
        Assert.AreEqual(-25f, world.Y);
    }

    [TestMethod]
    public void MouseWorldPosition_AllTransformsCombined_AppliesEach()
    {
        var view = (FakeCameraView)_camera.View;
        view.TileWidth = 4f;
        view.TileHeight = 4f;
        view.X = 10f;
        view.Y = 20f;
        _camera.X = 200f;
        _camera.Y = 100f;
        _mouse.ClientX = 240;
        _mouse.ClientY = 140;

        Point world = _camera.MouseWorldPosition;

        Assert.AreEqual(20f, world.X, delta: 0.001f); // (240 - 200)/4 + 10
        Assert.AreEqual(30f, world.Y, delta: 0.001f); // (140 - 100)/4 + 20
    }
}
