namespace BabyBearsEngine.Worlds;

/// <summary>
/// A holder for <see cref="IAddable"/> objects (entities, graphics, controllers, etc.).
/// Containers manage parent/child relationships, layered ordering for renderables, and the
/// local-to-window coordinate transform.
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
}
