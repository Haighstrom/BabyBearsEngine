namespace BabyBearsEngine.Worlds.Cameras;

/// <summary>
/// A camera view that shows a fixed region of world space. The tile-to-pixel scale adjusts
/// automatically when the camera is resized so the same world region always fills the viewport.
/// Set <see cref="ViewWidth"/> and <see cref="ViewHeight"/> to control how many world units are
/// visible, or set <see cref="ICameraView.TileWidth"/>/<see cref="ICameraView.TileHeight"/> to
/// drive the visible region from a pixel scale instead.
/// </summary>
public sealed class FreeCameraView : CameraView
{
    private float _viewH = 0f;
    private float _viewW = 0f;

    public FreeCameraView(float x, float y, float viewW, float viewH, Func<float> getCameraWidth, Func<float> getCameraHeight)
        : base(getCameraWidth, getCameraHeight)
    {
        X = x;
        Y = y;
        _viewW = viewW;
        _viewH = viewH;
    }

    /// <inheritdoc/>
    public override float TileHeight
    {
        get => _getCameraHeight() / _viewH;
        set { _viewH = _getCameraHeight() / value; RaiseViewChanged(); }
    }

    /// <inheritdoc/>
    public override float TileWidth
    {
        get => _getCameraWidth() / _viewW;
        set { _viewW = _getCameraWidth() / value; RaiseViewChanged(); }
    }

    /// <summary>
    /// Height of the world region shown by this camera, in world units. Setting this adjusts
    /// the tile-to-pixel scale so the new region fills the viewport height.
    /// </summary>
    public float ViewHeight
    {
        get => _viewH;
        set
        {
            _viewH = value;
            RaiseViewChanged();
        }
    }

    /// <summary>
    /// Width of the world region shown by this camera, in world units. Setting this adjusts
    /// the tile-to-pixel scale so the new region fills the viewport width.
    /// </summary>
    public float ViewWidth
    {
        get => _viewW;
        set
        {
            _viewW = value;
            RaiseViewChanged();
        }
    }
}
