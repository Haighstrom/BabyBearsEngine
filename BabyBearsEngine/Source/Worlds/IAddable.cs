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

    /// <summary>
    /// True when this addable is part of a live entity tree — i.e. walking the parent chain
    /// reaches a tree root (a non-<see cref="IAddable"/> <see cref="IContainer"/>, e.g. a
    /// <c>World</c>). False if this addable, or any of its ancestors, has been detached.
    /// </summary>
    /// <remarks>
    /// <para>For a leaf attached directly to a root container this matches "<see cref="Parent"/>
    /// is not null". For deeper trees it also catches the "I have a parent, but my parent (or
    /// further up) was detached" case — useful for skipping per-frame work on entities that are
    /// orphaned mid-update.</para>
    /// <para>Cost is O(depth-of-tree) per call — a pointer-chase up the parent chain. At
    /// typical scene depths this is negligible; if you only need the immediate-parent check
    /// inside a hot loop, test <c>Parent is not null</c> directly.</para>
    /// </remarks>
    bool Exists { get; }

    /// <summary>Raised immediately after this addable has been attached to a parent container.</summary>
    event EventHandler? Added;

    /// <summary>Raised immediately after this addable has been detached from its parent container.</summary>
    event EventHandler? Removed;

    /// <summary>Removes this addable from its current parent. Throws if it has no parent.</summary>
    void Remove();
}
