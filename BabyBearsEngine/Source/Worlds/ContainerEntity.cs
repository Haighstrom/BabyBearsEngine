using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Base for entities that themselves act as containers — i.e. anything that can hold child
/// entities/graphics. Inherits position and size from <see cref="AddableRectBase"/>, owns a
/// <see cref="Container"/> for children, and implements <see cref="ILayered"/> so the parent
/// container can sort it among its siblings.
/// <para><see cref="Update"/> walks active children; <see cref="Render"/> walks visible
/// children. Subclasses (e.g. <see cref="Entity"/>) typically override <see cref="Render"/>
/// to apply their own transform first.</para>
/// </summary>
public abstract class ContainerEntity : AddableRectBase, IEntity, IContainer, ILayered
{
    private readonly Container _container;
    private int _layer = 0;

    /// <summary>Creates a container entity at the origin with zero size and no children, active+visible defaults.</summary>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    protected ContainerEntity(int layer = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(layer);
        _layer = layer;
        _container = new Container(this);
    }

    /// <summary>Creates a container entity at (<paramref name="x"/>, <paramref name="y"/>) with the given size.</summary>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    protected ContainerEntity(float x, float y, float width, float height, int layer = 0)
        : base(x, y, width, height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(layer);
        _layer = layer;
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

    /// <inheritdoc/>
    public IList<IUpdateable> GetUpdatables() => _container.GetUpdatables();

    /// <inheritdoc/>
    public IList<IUpdateable> GetUpdatablesLast() => _container.GetUpdatablesLast();

    /// <inheritdoc/>
    public IList<IRenderable> GetRenderables() => _container.GetRenderables();

    /// <summary>
    /// Updates every active child <see cref="IUpdateable"/> that is part of the entity tree. Children with
    /// <see cref="IUpdateable.Active"/> = false, or whose <see cref="AddableBase.IsConnectedToTree"/> is false,
    /// are skipped. Override to insert custom per-frame logic; remember to call <c>base.Update(elapsed)</c> if
    /// you still want children to update.
    /// <para>Runs in two passes: every regular child first, then every child that opted into the post-pass
    /// via <see cref="IUpdateable.UpdateLast"/>. The post-pass exists for world-level coordinators (e.g.
    /// collision solvers) that need to observe child state once everything else has moved this frame.</para>
    /// <para>If a child's update detaches this entity (or any ancestor) from the tree, iteration stops — any
    /// remaining children would otherwise crash trying to access screen-space coordinates on a detached subtree.
    /// Because every <see cref="ContainerEntity"/> in the chain performs the same check, the bail-out propagates
    /// up correctly even when the removed ancestor is several levels above this one.</para>
    /// </summary>
    /// <param name="elapsed">Seconds since the last update.</param>
    public virtual void Update(double elapsed)
    {
        foreach (var entity in GetUpdatables())
        {
            if (!entity.Active || !entity.IsConnectedToTree)
            {
                continue;
            }

            entity.Update(elapsed);

            if (!IsConnectedToTree)
            {
                break;
            }
        }

        foreach (var entity in GetUpdatablesLast())
        {
            if (!entity.Active || !entity.IsConnectedToTree)
            {
                continue;
            }

            entity.Update(elapsed);

            if (!IsConnectedToTree)
            {
                break;
            }
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
    public void Add(params IAddable[] children) => _container.Add(children);

    /// <inheritdoc/>
    public void Add(IAddable entity) => _container.Add(entity);

    /// <inheritdoc/>
    public void Remove(IAddable entity) => _container.Remove(entity);

    /// <inheritdoc/>
    public void RemoveAll() => _container.RemoveAll();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="AddableBase.Parent"/> is <c>null</c> — a container entity outside the entity tree has no window-space position to translate into.</exception>
    public virtual (float x, float y) GetWindowCoordinates(float x, float y) =>
        Parent?.GetWindowCoordinates(x, y) ?? throw new InvalidOperationException("GetWindowCoordinates requires Parent — entity is not in an entity tree (never added, or removed).");
}
