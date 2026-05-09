using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Graphics;

/// <summary>
/// Base class for renderable graphics (images, shapes, text). Combines the addable plumbing from
/// <see cref="AddableBase"/> with layered ordering (<see cref="ILayered"/>) and visibility, leaving
/// concrete subclasses to implement the actual <see cref="Render"/> call.
/// </summary>
public abstract class GraphicBase : AddableBase, IRenderable, ILayered
{
    private int _layer;

    /// <inheritdoc/>
    public int Layer => _layer;

    /// <inheritdoc/>
    public bool Visible { get; set; } = true;

    /// <inheritdoc/>
    public event EventHandler<LayerChangedEventArgs>? LayerChanged;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public abstract void Render(ref Matrix3 projection, ref Matrix3 modelView);
}
