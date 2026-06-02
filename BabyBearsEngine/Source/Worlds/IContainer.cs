namespace BabyBearsEngine.Worlds;

/// <summary>
/// A holder for <see cref="IAddable"/> objects (entities, graphics, controllers, etc.).
/// Containers manage parent/child relationships, layered ordering for renderables and
/// updateables, and the local-to-window coordinate transform.
/// </summary>
public interface IContainer
{
    /// <summary>Adds <paramref name="entity"/> to this container and sets its parent to this.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="entity"/> is already a child of this container.</exception>
    void Add(IAddable entity);

    /// <summary>Adds each element of <paramref name="children"/> to this container in order.</summary>
    void Add(params IAddable[] children)
    {
        foreach (var child in children)
        {
            Add(child);
        }
    }

    /// <summary>Removes <paramref name="entity"/> from this container and clears its parent.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="entity"/> is not a child of this container.</exception>
    void Remove(IAddable entity);

    /// <summary>Removes every child from this container.</summary>
    void RemoveAll();

    /// <summary>
    /// Translates a local (container-relative) point into window coordinates by walking up the parent chain.
    /// Each container adds its own offset and delegates upward; the root (typically the <see cref="IWorld"/>)
    /// returns the input unchanged.
    /// </summary>
    (float x, float y) GetWindowCoordinates(float x, float y);

    /// <summary>
    /// Returns a snapshot of the child <see cref="IRenderable"/>s in render order: highest
    /// <see cref="ILayered.Layer"/> first (drawn behind), lowest last (drawn on top). Renderables
    /// that don't implement <see cref="ILayered"/> are treated as <c>int.MaxValue</c> (rendered
    /// first / furthest back). Equal layers are ordered by insertion — earlier-added renders
    /// first, so later adds overlay. Safe to mutate the container while iterating the returned
    /// list.
    /// </summary>
    /// <remarks>Default implementation returns an empty list — only containers that actually host renderables (e.g. <see cref="Container"/>) need override.</remarks>
    IList<IRenderable> GetRenderables() => [];

    /// <summary>
    /// Returns a snapshot of the child <see cref="IUpdateable"/>s in update order (highest layer
    /// first, so the top-most child updates last). Excludes children that opted into the post-pass
    /// via <see cref="IUpdateable.UpdateLast"/> — use <see cref="GetUpdatablesLast"/> for those.
    /// Safe to mutate the container while iterating the returned list.
    /// </summary>
    /// <remarks>Default implementation returns an empty list — only containers that actually host updateables need override.</remarks>
    IList<IUpdateable> GetUpdatables() => [];

    /// <summary>
    /// Returns a snapshot of the child post-pass <see cref="IUpdateable"/>s (those with
    /// <see cref="IUpdateable.UpdateLast"/> = <c>true</c>) in update order. Ticked after every
    /// regular child in <see cref="GetUpdatables"/> has finished. Safe to mutate the container
    /// while iterating.
    /// </summary>
    /// <remarks>Default implementation returns an empty list — only containers that actually host updateables need override.</remarks>
    IList<IUpdateable> GetUpdatablesLast() => [];
}
