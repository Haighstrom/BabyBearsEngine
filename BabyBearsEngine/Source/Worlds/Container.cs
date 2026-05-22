using System.Collections.Generic;
using System.Diagnostics;
using BabyBearsEngine.Worlds.Graphics;

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

    // Authoritative collection of added items. Other lists are derived
    // convenience views for rendering and updating.
    private readonly List<IAddable> _children = [];
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    public IList<IUpdateable> GetUpdatables() => [.. _updateables];

    public IList<IRenderable> GetRenderables() => [.. _graphics];

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
        if (_children.Contains(entity))
        {
            throw new InvalidOperationException("Entity is already added to this container.");
        }

        _children.Add(entity);

        if (entity is IUpdateable updatable)
        {
            InsertUpdateable(updatable);
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
        if (!_children.Contains(entity))
        {
            throw new InvalidOperationException("Entity is not present in this container.");
        }

        _children.Remove(entity);

        if (entity is IUpdateable updatable)
        {
            _updateables.Remove(updatable);
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
        if (_children.Count != 0 || _updateables.Count != 0 || _graphics.Count != 0)
        {
            Debug.Fail("RemoveAll: internal lists not empty after Remove(child) loop");
        }

        // Ensure auxiliary lists are empty.
        _updateables.Clear();
        _graphics.Clear();
        _children.Clear();
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
    // the entity drawn on top (lowest layer) updates last.
    private void InsertUpdateable(IUpdateable updateable)
    {
        int layer = (updateable as ILayered)?.Layer ?? NonLayeredLayer;

        for (int i = 0; i < _updateables.Count; i++)
        {
            if (layer > ((_updateables[i] as ILayered)?.Layer ?? NonLayeredLayer))
            {
                _updateables.Insert(i, updateable);
                return;
            }
        }

        _updateables.Add(updateable);
    }

    private void OnLayerChanged(object? sender, LayerChangedEventArgs _)
    {
        if (sender is IRenderable renderable)
        {
            _graphics.Remove(renderable);
            InsertRenderable(renderable);
        }

        if (sender is IUpdateable updateable)
        {
            _updateables.Remove(updateable);
            InsertUpdateable(updateable);
        }
    }
}
