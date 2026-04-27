namespace BabyBearsEngine.Worlds.Cameras;

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

    public override float TileHeight
    {
        get => _getCameraHeight() / _viewH;
        set { _viewH = _getCameraHeight() / value; RaiseViewChanged(); }
    }

    public override float TileWidth
    {
        get => _getCameraWidth() / _viewW;
        set { _viewW = _getCameraWidth() / value; RaiseViewChanged(); }
    }

    public float ViewHeight
    {
        get => _viewH;
        set
        {
            _viewH = value;
            RaiseViewChanged();
        }
    }

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
