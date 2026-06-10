using System.Diagnostics;

namespace BabyBearsEngine.Worlds;

// Internal helper used by ContainerEntity / World to share the actual storage and ordering
// logic. realParent is the public-facing container that gets passed to entities as their
// parent and that handles coordinate translation up the tree.
internal class Container(IContainer realParent) : IContainer
{
    // Layer assigned to children that do not implement ILayered, used for both
    // render-order and update-order sorting. int.MaxValue places them at the very
    // back: drawn behind all layered content, and updated first so their mouse-input
    // registration is overridden by any layered content in front of them (e.g. an
    // Entity's internal ClickController never steals clicks from the Entity's own
    // child content). Matches the BearsEngine default, where unlayered items sat at
    // the back rather than on top.
    private const int NonLayeredLayer = int.MaxValue;

    // Membership only — insertion order, not z-order. Use _graphics / _updateables for layer-sorted iteration.
    private readonly List<IAddable> _children = [];
    // Mirrors _children for O(1) membership tests on Add/Remove, avoiding a linear scan per call
    // (which adds up when spawning thousands of small entities). Kept in lock-step with _children.
    private readonly HashSet<IAddable> _childrenSet = [];
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    // Updateables that opted in to the post-pass via IUpdateable.UpdateLast. Updated by
    // World / ContainerEntity after every regular updateable in this container has ticked,
    // for world-level coordinators (e.g. CollisionSolver) that need to observe entity state
    // once everything else has moved this frame. Routed by UpdateLast snapshotted at Add.
    private readonly List<IUpdateable> _updateablesLast = [];

    // Reused scratch buffers for the Snapshot* accessors, so the per-frame Update/Render loops
    // don't allocate a fresh List per container per frame. Each is refilled on demand and is only
    // valid until the next call to the same accessor — safe because no caller re-enters the same
    // container's same accessor while iterating it (the loops descend into child containers, each
    // of which has its own buffers).
    private readonly List<IUpdateable> _updateablesSnapshot = [];
    private readonly List<IUpdateable> _updateablesLastSnapshot = [];
    private readonly List<IRenderable> _graphicsSnapshot = [];

    // Guards OnLayerChanged against re-entrant layer mutation. OnLayerChanged re-sorts the lists in
    // place; if a layer is changed again while it is mid-sort (a LayerChanged handler that writes
    // another layer, or a side-effecting Layer getter), the list would be left partially sorted.
    private bool _resorting = false;

    /// <inheritdoc/>
    public IList<IUpdateable> GetUpdatables() => [.. _updateables];

    /// <inheritdoc/>
    public IList<IUpdateable> GetUpdatablesLast() => [.. _updateablesLast];

    /// <inheritdoc/>
    public IList<IRenderable> GetRenderables() => [.. _graphics];

    // Internal hot-path counterparts to the public Get* accessors above. They return a reused
    // buffer (see the snapshot field notes) instead of allocating a fresh List each frame, so the
    // caller must iterate the result before the next call to the same accessor and must not retain
    // or mutate it. Used by the per-frame Update/Render loops in ContainerEntity and World.
    internal List<IUpdateable> SnapshotUpdatables()
    {
        _updateablesSnapshot.Clear();
        _updateablesSnapshot.AddRange(_updateables);
        return _updateablesSnapshot;
    }

    internal List<IUpdateable> SnapshotUpdatablesLast()
    {
        _updateablesLastSnapshot.Clear();
        _updateablesLastSnapshot.AddRange(_updateablesLast);
        return _updateablesLastSnapshot;
    }

    internal List<IRenderable> SnapshotRenderables()
    {
        _graphicsSnapshot.Clear();
        _graphicsSnapshot.AddRange(_graphics);
        return _graphicsSnapshot;
    }

    public (float x, float y) GetWindowCoordinates(float x, float y) =>
        realParent.GetWindowCoordinates(x, y);

    public void Add(params IAddable[] children)
    {
        foreach (var child in children)
        {
            Add(child);
        }
    }

    public void Add(IAddable entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Reject duplicate adds — caller should not add the same entity twice.
        if (_childrenSet.Contains(entity))
        {
            throw new InvalidOperationException("Entity is already added to this container.");
        }

        _children.Add(entity);
        _childrenSet.Add(entity);

        if (entity is IUpdateable updatable)
        {
            // Snapshot UpdateLast here — mutation afterwards is not observed (documented on IUpdateable).
            InsertUpdateable(updatable, updatable.UpdateLast ? _updateablesLast : _updateables);
        }

        if (entity is IRenderable renderable)
        {
            InsertRenderable(renderable);
        }

        if (entity is ILayered layered)
        {
            layered.LayerChanged += OnLayerChanged;
        }

        entity.Parent = realParent;
    }

    public void Remove(IAddable entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // If the entity isn't tracked, that's a misuse of the API — throw.
        if (!_childrenSet.Contains(entity))
        {
            throw new InvalidOperationException("Entity is not present in this container.");
        }

        _children.Remove(entity);
        _childrenSet.Remove(entity);

        if (entity is IUpdateable updatable)
        {
            // Try both lists since UpdateLast is snapshotted at Add and we don't track
            // which bucket each entry went into separately.
            if (!_updateables.Remove(updatable))
            {
                _updateablesLast.Remove(updatable);
            }
        }

        if (entity is IRenderable renderable)
        {
            _graphics.Remove(renderable);
        }

        if (entity is ILayered layered)
        {
            layered.LayerChanged -= OnLayerChanged;
        }

        entity.Parent = null;
    }

    public void RemoveAll()
    {
        // Use the authoritative children list to remove everything. Call
        // Remove for each child to reuse the single-entity removal logic and
        // ensure parents are cleared consistently.
        foreach (var child in _children.ToArray())
        {
            Remove(child);
        }

        // Debug-time check: if anything remains it indicates a logic bug in
        // Remove(child). Surface this during development rather than
        // silently masking it. Still clear the lists to keep runtime state
        // consistent.
        if (_children.Count != 0 || _childrenSet.Count != 0 || _updateables.Count != 0 || _updateablesLast.Count != 0 || _graphics.Count != 0)
        {
            Debug.Fail("RemoveAll: internal lists not empty after Remove(child) loop");
        }

        // Ensure auxiliary lists are empty.
        _updateables.Clear();
        _updateablesLast.Clear();
        _graphics.Clear();
        _children.Clear();
        _childrenSet.Clear();
    }

    private void InsertRenderable(IRenderable renderable)
    {
        int layer = (renderable as ILayered)?.Layer ?? NonLayeredLayer;

        for (int i = 0; i < _graphics.Count; i++)
        {
            if (layer > ((_graphics[i] as ILayered)?.Layer ?? NonLayeredLayer))
            {
                _graphics.Insert(i, renderable);
                return;
            }
        }

        _graphics.Add(renderable);
    }

    // Updateables are kept in the same layer order as renderables, so that per-frame
    // update — and therefore mouse-input registration — follows the visual stacking:
    // the entity drawn on top (lowest layer) updates last. The same ordering applies
    // independently within each of the two buckets (regular and last-pass).
    private static void InsertUpdateable(IUpdateable updateable, List<IUpdateable> bucket)
    {
        int layer = (updateable as ILayered)?.Layer ?? NonLayeredLayer;

        for (int i = 0; i < bucket.Count; i++)
        {
            if (layer > ((bucket[i] as ILayered)?.Layer ?? NonLayeredLayer))
            {
                bucket.Insert(i, updateable);
                return;
            }
        }

        bucket.Add(updateable);
    }

    private void OnLayerChanged(object? sender, LayerChangedEventArgs _)
    {
        // Fail loudly rather than silently corrupting render/update order if a layer is changed
        // while this handler is mid-sort. Nothing in the engine does this today, but a future
        // controller that writes a layer from inside a LayerChanged handler would otherwise leave
        // a list partially sorted with no symptom until something renders in the wrong order.
        if (_resorting)
        {
            throw new InvalidOperationException("A child's Layer changed while the container was already re-sorting after a layer change. Re-entrant layer changes from within a LayerChanged handler are not supported — defer the change (e.g. to the next Update).");
        }

        _resorting = true;

        if (sender is IRenderable renderable)
        {
            _graphics.Remove(renderable);
            InsertRenderable(renderable);
        }

        if (sender is IUpdateable updateable)
        {
            // Re-sort within whichever bucket the item is already in, preserving its
            // original UpdateLast routing (which is a snapshot from Add).
            if (_updateables.Remove(updateable))
            {
                InsertUpdateable(updateable, _updateables);
            }
            else if (_updateablesLast.Remove(updateable))
            {
                InsertUpdateable(updateable, _updateablesLast);
            }
        }

        _resorting = false;
    }
}
