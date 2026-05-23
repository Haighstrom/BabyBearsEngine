using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Base class for renderable graphics (images, shapes, text). Inherits position and size from
/// <see cref="AddableRectBase"/>, adds visibility and layered ordering on top, and leaves the
/// actual <see cref="Render"/> call to concrete subclasses.
/// </summary>
public abstract class GraphicBase : AddableRectBase, IRenderable, ILayered
{
    private int _layer = 0;

    /// <summary>Creates a graphic at the origin with zero size.</summary>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    protected GraphicBase(int layer = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(layer);
        _layer = layer;
    }

    /// <summary>Creates a graphic at (<paramref name="x"/>, <paramref name="y"/>) with the given size.</summary>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    protected GraphicBase(float x, float y, float width, float height, int layer = int.MaxValue)
        : base(x, y, width, height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(layer);
        _layer = layer;
    }

    /// <inheritdoc/>
    public int Layer
    {
        get => _layer;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            int old = _layer;
            _layer = value;
            if (old != value)
            {
                LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
            }
        }
    }

    /// <inheritdoc/>
    public bool Visible { get; set; } = true;

    /// <inheritdoc/>
    public event EventHandler<LayerChangedEventArgs>? LayerChanged;

    /// <inheritdoc/>
    public abstract void Render(ref Matrix3 projection, ref Matrix3 modelView);
}
