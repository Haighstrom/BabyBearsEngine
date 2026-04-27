namespace BabyBearsEngine.Worlds.Cameras;

public abstract class CameraView(Func<float> getCameraWidth, Func<float> getCameraHeight)
{
    protected readonly Func<float> _getCameraHeight = getCameraHeight;
    protected readonly Func<float> _getCameraWidth = getCameraWidth;
    private float _x = 0f;
    private float _y = 0f;

    public abstract float TileHeight { get; set; }
    public abstract float TileWidth { get; set; }

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            RaiseViewChanged();
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            RaiseViewChanged();
        }
    }

    public event EventHandler? ViewChanged;

    protected void RaiseViewChanged() => ViewChanged?.Invoke(this, EventArgs.Empty);

    public (float x, float y) WorldToLocal(float worldX, float worldY)
    {
        return ((worldX - _x) * TileWidth, (worldY - _y) * TileHeight);
    }
}
