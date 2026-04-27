namespace BabyBearsEngine.Worlds.Cameras;

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

    public override float TileHeight
    {
        get => _tileHeight;
        set
        {
            _tileHeight = value;
            RaiseViewChanged();
        }
    }

    public override float TileWidth
    {
        get => _tileWidth;
        set
        {
            _tileWidth = value;
            RaiseViewChanged();
        }
    }

    public float ViewHeight => _getCameraHeight() / _tileHeight;
    public float ViewWidth => _getCameraWidth() / _tileWidth;
}
