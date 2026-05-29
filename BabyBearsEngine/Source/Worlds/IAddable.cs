namespace BabyBearsEngine.Worlds;

/// <summary>
/// Anything that can be added to and removed from an <see cref="IContainer"/>.
/// Tracks its parent container and exposes a self-removal helper.
/// </summary>
public interface IAddable
{
    /// <summary>
    /// The container this addable currently belongs to, or <c>null</c> if it has not been added.
    /// Containers assign this on add/remove; gameplay code should not assign it directly.
    /// Switching directly from one parent to another is not allowed — set to <c>null</c> first.
    /// </summary>
    IContainer? Parent { get; set; }

    /// <summary>True when this addable is currently inside a container (equivalent to <see cref="Parent"/> being non-null).</summary>
    bool Exists { get; }

    /// <summary>
    /// True when walking the parent chain from this addable reaches a tree root (a non-<see cref="IAddable"/>
    /// <see cref="IContainer"/>, e.g. a <c>World</c>). False when any link in the chain is <c>null</c> — i.e.
    /// this addable, or one of its ancestors, has been detached. Distinct from <see cref="Exists"/>, which only
    /// checks the immediate parent.
    /// </summary>
    /// <remarks>
    /// Cost is O(depth-of-tree) per call — a pointer-chase up the parent chain. At typical scene depths
    /// this is negligible; revisit if profiling shows otherwise.
    /// </remarks>
    bool IsConnectedToTree
    {
        get
        {
            for (IAddable? current = this; current is not null; current = current.Parent as IAddable)
            {
                if (current.Parent is null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>Raised immediately after this addable has been attached to a parent container.</summary>
    event EventHandler? Added;

    /// <summary>Raised immediately after this addable has been detached from its parent container.</summary>
    event EventHandler? Removed;

    /// <summary>Removes this addable from its current parent. Throws if it has no parent.</summary>
    void Remove();
}
