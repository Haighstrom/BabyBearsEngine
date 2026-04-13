using System.Collections.Generic;
using System.Diagnostics;
using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

internal class Container() : IContainer
{
    // Authoritative collection of added items. Other lists are derived
    // convenience views for rendering and updating.
    private readonly List<IAddable> _children = [];
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    public IList<IUpdateable> GetUpdatables() => [.. _updateables];

    public IList<IRenderable> GetRenderables() => [.. _graphics];

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
            _graphics.Add(renderable);
        }

        entity.SetParent(this);
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
}
