using System.Collections.Generic;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Base for entities that themselves act as containers — i.e. anything that can hold child entities/graphics.
/// Combines <see cref="IEntity"/>, <see cref="IContainer"/>, and <see cref="ILayered"/> in one type.
/// <para><see cref="Update"/> walks active children; <see cref="Render"/> walks visible children. Subclasses
/// (e.g. <see cref="Entity"/>) typically override <see cref="Render"/> to apply their own transform first.</para>
/// </summary>
public abstract class ContainerEntity : AddableBase, IEntity, IContainer, ILayered
{
    private readonly Container _container;
    private int _layer;

    /// <summary>Creates a container entity with no children, layer 0, and active+visible defaults.</summary>
    protected ContainerEntity()
    {
        _container = new Container(this);
    }

    /// <inheritdoc/>
    public bool Active { get; set; } = true;

    /// <inheritdoc/>
    public bool Visible { get; set; } = true;

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
    public event EventHandler<LayerChangedEventArgs>? LayerChanged;

    /// <summary>Returns a snapshot of the child <see cref="IUpdateable"/>s. Safe to mutate the container while iterating the returned list.</summary>
    protected IList<IUpdateable> GetUpdatables() => _container.GetUpdatables();

    /// <summary>Returns a snapshot of the child <see cref="IRenderable"/>s in render order (highest layer first). Safe to mutate the container while iterating.</summary>
    protected IList<IRenderable> GetRenderables() => _container.GetRenderables();

    /// <summary>
    /// Updates every active child <see cref="IUpdateable"/>. Children with <see cref="IUpdateable.Active"/> = false are skipped.
    /// Override to insert custom per-frame logic; remember to call <c>base.Update(elapsed)</c> if you still want children to update.
    /// </summary>
    /// <param name="elapsed">Seconds since the last update.</param>
    public virtual void Update(double elapsed)
    {
        foreach (var entity in GetUpdatables())
        {
            if (!entity.Active)
            {
                continue;
            }

            entity.Update(elapsed);
        }
    }

    /// <summary>
    /// Renders every visible child <see cref="IRenderable"/> in layer order. Children with <see cref="IRenderable.Visible"/> = false are skipped.
    /// Override to apply your own transform (e.g. translation) before delegating to <c>base.Render</c>.
    /// </summary>
    public virtual void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        foreach (var entity in GetRenderables())
        {
            if (!entity.Visible)
            {
                continue;
            }

            entity.Render(ref projection, ref modelView);
        }
    }

    /// <inheritdoc/>
    public void Add(IAddable entity) => _container.Add(entity);

    /// <inheritdoc/>
    public void Remove(IAddable entity) => _container.Remove(entity);

    /// <inheritdoc/>
    public void RemoveAll() => _container.RemoveAll();

    /// <inheritdoc/>
    public virtual (float x, float y) GetWindowCoordinates(float x, float y) =>
        Parent?.GetWindowCoordinates(x, y) ?? (x, y);
}
