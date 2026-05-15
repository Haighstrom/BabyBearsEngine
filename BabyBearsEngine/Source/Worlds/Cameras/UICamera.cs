using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// A camera that renders its children at a fixed 1:1 pixel-to-screen mapping with no
/// tile scaling, scrolling, or world-space transforms. Intended for UI overlays that must
/// stay in screen space regardless of what the game camera is doing.
/// </summary>
public sealed class UICamera : ContainerEntity, ICamera
{
    private readonly Cameras.CameraRenderer _renderer;
    private readonly Cameras.FixedTileSizeCameraView _view;

    public UICamera(float x, float y, float width, float height, MsaaSamples samples = MsaaSamples.Disabled)
        : base(x, y, width, height)
    {
        MSAASamples = samples;
        _view = new Cameras.FixedTileSizeCameraView(1, 1, () => Width, () => Height);
        _renderer = new Cameras.CameraRenderer(width, height, samples);
    }

    /// <inheritdoc/>
    public Colour BackgroundColour { get; set; } = Colour.Transparent;

    /// <inheritdoc/>
    public float GameSpeed { get; set; } = 1;

    /// <inheritdoc/>
    public bool MouseIntersecting
    {
        get
        {
            var (wx, wy) = Parent?.GetWindowCoordinates(X, Y) ?? (X, Y);
            return new Rect(wx, wy, Width, Height).Contains(Mouse.ClientX, Mouse.ClientY);
        }
    }

    /// <inheritdoc/>
    public MsaaSamples MSAASamples { get; set; }

    /// <inheritdoc/>
    public Cameras.CameraView View => _view;

    /// <inheritdoc/>
    public event EventHandler? ViewChanged
    {
        add => _view.ViewChanged += value;
        remove => _view.ViewChanged -= value;
    }

    /// <summary>
    /// Translates a local point to window space. Since UICamera uses 1:1 pixel coordinates,
    /// this is a simple offset by the camera's screen position.
    /// </summary>
    public override (float x, float y) GetWindowCoordinates(float x, float y)
    {
        float sx = X + x;
        float sy = Y + y;
        return Parent?.GetWindowCoordinates(sx, sy) ?? (sx, sy);
    }

    /// <inheritdoc/>
    public override void Update(double elapsed) => base.Update(elapsed * GameSpeed);

    /// <inheritdoc/>
    public override void Render(ref Matrix3 projection, ref Matrix3 modelView) =>
        _renderer.Render(this, GetRenderables(), ref projection, ref modelView);
}
