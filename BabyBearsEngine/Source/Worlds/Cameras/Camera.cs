using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Worlds;

public sealed class Camera : ContainerEntity
{
    private readonly CameraView _cameraView;
    private readonly CameraRenderer _renderer;

    private Camera(float x, float y, float width, float height, Func<Func<float>, Func<float>, CameraView> createView, MsaaSamples samples = MsaaSamples.Disabled)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MSAASamples = samples;

        _cameraView = createView(() => Width, () => Height);
        _renderer = new CameraRenderer(width, height, samples);
    }

    public static Camera WithTileSize(float x, float y, float width, float height, float tileW, float tileH, MsaaSamples samples = MsaaSamples.Disabled)
        => new(x, y, width, height, (getW, getH) => new FixedTileSizeCameraView(tileW, tileH, getW, getH), samples);

    public static Camera WithView(float x, float y, float width, float height, float viewX, float viewY, float viewW, float viewH, MsaaSamples samples = MsaaSamples.Disabled)
        => new(x, y, width, height, (getW, getH) => new FreeCameraView(viewX, viewY, viewW, viewH, getW, getH), samples);

    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Colour BackgroundColour { get; set; } = Colour.White;

    public float GameSpeed { get; set; } = 1;

    public MsaaSamples MSAASamples { get; set; } = MsaaSamples.Disabled;

    public CameraView View => _cameraView;


    public event EventHandler? ViewChanged
    {
        add => _cameraView.ViewChanged += value;
        remove => _cameraView.ViewChanged -= value;
    }

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
