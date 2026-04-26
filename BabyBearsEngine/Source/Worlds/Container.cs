using System.Collections.Generic;
using System.Diagnostics;
using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

internal class Container(IContainer realParent) : IContainer
{
    // Layer assigned to IRenderable objects that do not implement ILayered.
    private const int NonLayeredRenderableLayer = 0;

    // Authoritative collection of added items. Other lists are derived
    // convenience views for rendering and updating.
    private readonly List<IAddable> _children = [];
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    public IList<IUpdateable> GetUpdatables() => [.. _updateables];

    public IList<IRenderable> GetRenderables() => [.. _graphics];

    public (float x, float y) GetWindowCoordinates(float x, float y) =>
        realParent.GetWindowCoordinates(x, y);

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
            _updateables.Add(updatable);
        }

        if (entity is IRenderable renderable)
        {
            InsertRenderable(renderable);

            if (entity is ILayered layered)
            {
                layered.LayerChanged += OnLayerChanged;
            }
        }

        entity.SetParent(realParent);
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

            if (entity is ILayered layered)
            {
                layered.LayerChanged -= OnLayerChanged;
            }
        }

        entity.SetParent(null);
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
        int layer = (renderable as ILayered)?.Layer ?? NonLayeredRenderableLayer;

        for (int i = 0; i < _graphics.Count; i++)
        {
            if (layer > ((_graphics[i] as ILayered)?.Layer ?? NonLayeredRenderableLayer))
            {
                _graphics.Insert(i, renderable);
                return;
            }
        }

        _graphics.Add(renderable);
    }

    private void OnLayerChanged(object? sender, LayerChangedEventArgs _)
    {
        var renderable = (IRenderable)sender!;
        _graphics.Remove(renderable);
        InsertRenderable(renderable);
    }
}
