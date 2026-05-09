namespace BabyBearsEngine.Worlds;

/// <summary>
/// Anything that can be added to and removed from an <see cref="IContainer"/>.
/// Tracks its parent container and exposes a self-removal helper.
/// </summary>
public interface IAddable
{
    /// <summary>The container this addable currently belongs to, or <c>null</c> if it has not been added.</summary>
    IContainer? Parent { get; }

    /// <summary>True when this addable is currently inside a container (equivalent to <see cref="Parent"/> being non-null).</summary>
    bool Exists { get; }

    /// <summary>
    /// Sets or clears the parent container. Containers call this on add/remove; gameplay code should not invoke it directly.
    /// Switching directly from one parent to another is not allowed — pass <c>null</c> first.
    /// </summary>
    /// <param name="parent">The new parent, or <c>null</c> to detach.</param>
    void SetParent(IContainer? parent);

    /// <summary>Removes this addable from its current parent. Throws if it has no parent.</summary>
    void Remove();
}
