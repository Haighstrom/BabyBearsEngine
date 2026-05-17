namespace BabyBearsEngine.Worlds.Cameras;

/// <summary>
/// A camera view with a fixed world-to-pixel tile size. The region of world space visible
/// through the camera grows and shrinks with the camera viewport — resizing the camera reveals
/// more or fewer tiles rather than stretching the image. Use <see cref="ViewWidth"/> and
/// <see cref="ViewHeight"/> to read the currently visible world region.
/// </summary>
public sealed class FixedTileSizeCameraView : CameraView
{
    private float _tileHeight = 0f;
    private float _tileWidth = 0f;

    public FixedTileSizeCameraView(float tileWidth, float tileHeight, Func<float> getCameraWidth, Func<float> getCameraHeight)
        : base(getCameraWidth, getCameraHeight)
    {
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
    }

    /// <inheritdoc/>
    public override float TileHeight
    {
        get => _tileHeight;
        set
        {
            _tileHeight = value;
            RaiseViewChanged();
        }
    }

    /// <inheritdoc/>
    public override float TileWidth
    {
        get => _tileWidth;
        set
        {
            _tileWidth = value;
            RaiseViewChanged();
        }
    }

    /// <summary>Height of the world region currently visible through the camera, in world units.</summary>
    public float ViewHeight => _getCameraHeight() / _tileHeight;

    /// <summary>Width of the world region currently visible through the camera, in world units.</summary>
    public float ViewWidth => _getCameraWidth() / _tileWidth;
}
