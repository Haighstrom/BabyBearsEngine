using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// A world-space camera that renders its child entities onto a viewport on screen. The camera
/// applies a <see cref="CameraView"/> transform so world coordinates map to pixels within its
/// bounds. Use <see cref="WithTileSize"/> for a fixed pixel-per-tile scale or
/// <see cref="WithView"/> to show a fixed world-space region.
/// </summary>
public class Camera : ContainerEntity, ICamera
{
    private readonly CameraView _cameraView;
    private readonly CameraRenderer _renderer;

    protected Camera(float x, float y, float width, float height, Func<Func<float>, Func<float>, CameraView> createView, MsaaSamples? samples = null)
        : base(x, y, width, height)
    {
        MsaaSamples effectiveSamples = samples ?? EngineConfiguration.DefaultCameraMsaa;
        MSAASamples = effectiveSamples;

        _cameraView = createView(() => Width, () => Height);
        _renderer = new CameraRenderer(width, height, effectiveSamples);
    }

    /// <summary>
    /// Creates a camera with a fixed world-to-pixel tile size. The number of world tiles
    /// visible changes when the camera is resized.
    /// </summary>
    /// <param name="x">Camera X position in the parent's coordinate space.</param>
    /// <param name="y">Camera Y position in the parent's coordinate space.</param>
    /// <param name="width">Camera viewport width in pixels.</param>
    /// <param name="height">Camera viewport height in pixels.</param>
    /// <param name="tileW">Width of one world tile in pixels.</param>
    /// <param name="tileH">Height of one world tile in pixels.</param>
    /// <param name="samples">MSAA sample count for the render target. When omitted, uses <see cref="ApplicationSettings.DefaultCameraMsaa"/>.</param>
    public static Camera WithTileSize(float x, float y, float width, float height, float tileW, float tileH, MsaaSamples? samples = null)
        => new(x, y, width, height, (getW, getH) => new FixedTileSizeCameraView(tileW, tileH, getW, getH), samples);

    /// <summary>
    /// Creates a camera that shows a fixed region of world space. The tile size scales
    /// automatically so the chosen world region always fills the viewport.
    /// </summary>
    /// <param name="x">Camera X position in the parent's coordinate space.</param>
    /// <param name="y">Camera Y position in the parent's coordinate space.</param>
    /// <param name="width">Camera viewport width in pixels.</param>
    /// <param name="height">Camera viewport height in pixels.</param>
    /// <param name="viewX">World-space X coordinate of the left edge of the view.</param>
    /// <param name="viewY">World-space Y coordinate of the top edge of the view.</param>
    /// <param name="viewW">Width of the world region to display.</param>
    /// <param name="viewH">Height of the world region to display.</param>
    /// <param name="samples">MSAA sample count for the render target. When omitted, uses <see cref="ApplicationSettings.DefaultCameraMsaa"/>.</param>
    public static Camera WithView(float x, float y, float width, float height, float viewX, float viewY, float viewW, float viewH, MsaaSamples? samples = null)
        => new(x, y, width, height, (getW, getH) => new FreeCameraView(viewX, viewY, viewW, viewH, getW, getH), samples);

    public Colour BackgroundColour { get; set; } = Colour.White;

    public float GameSpeed { get; set; } = 1;

    public float MaxX { get; set; } = 0f;

    public float MaxY { get; set; } = 0f;

    public float MinX { get; set; } = 0f;

    public float MinY { get; set; } = 0f;

    public bool MouseIntersecting
    {
        get
        {
            var (wx, wy) = Parent?.GetWindowCoordinates(X, Y) ?? (X, Y);
            return new Rect(wx, wy, Width, Height).Contains(Mouse.ClientX, Mouse.ClientY);
        }
    }

    public MsaaSamples MSAASamples { get; set; } = MsaaSamples.Disabled;

    public ICameraView View => _cameraView;


    public event EventHandler? ViewChanged
    {
        add => _cameraView.ViewChanged += value;
        remove => _cameraView.ViewChanged -= value;
    }

    /// <summary>
    /// Converts a world-space point to screen (window) coordinates by applying the camera's
    /// view transform and then this camera's position within the entity hierarchy.
    /// </summary>
    public override (float x, float y) GetWindowCoordinates(float x, float y)
    {
        var (lx, ly) = _cameraView.WorldToLocal(x, y);
        float sx = X + lx;
        float sy = Y + ly;
        return Parent?.GetWindowCoordinates(sx, sy) ?? (sx, sy);
    }

    public override void Update(double elapsed) => base.Update(elapsed * GameSpeed);

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView) => _renderer.Render(this, GetRenderables(), ref projection, ref modelView);
}
