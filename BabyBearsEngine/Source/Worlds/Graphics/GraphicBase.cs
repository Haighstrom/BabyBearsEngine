using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Graphics;

public abstract class GraphicBase : AddableBase, IRenderable, ILayered
{
    private int _layer;

    public int Layer => _layer;

    public bool Visible { get; set; } = true;

    public event EventHandler<LayerChangedEventArgs>? LayerChanged;

    public void SetLayer(int layer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(layer);
        int old = _layer;
        _layer = layer;
        if (old != layer)
        {
            LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, layer));
        }
    }

    public abstract void Render(ref Matrix3 projection, ref Matrix3 modelView);
}
