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
