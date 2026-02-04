using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

public class World() : IWorld
{
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    public void Load()
    {
    }

    public void Unload()
    {
        //foreach (var graphic in _graphics.ToList())
        //{
        //    graphic.Dispose();
        //}
    }

    public void Clear()
    {
        //dispose?
        _graphics.Clear();
        _updateables.Clear();
    }

    public void Add(IRenderable graphic)
    {
        _graphics.Add(graphic);
    }

    public void Add(IUpdateable updateable)
    {
        _updateables.Add(updateable);
    }

    public void Add(IEntity entity)
    {
        _graphics.Add(entity);
        _updateables.Add(entity);
    }

    public void Remove(IRenderable graphic)
    {
        _graphics.Remove(graphic);
    }

    public void Remove(IUpdateable updateable)
    {
        _updateables.Remove(updateable);
    }

    public void Remove(IEntity entity)
    {
        _graphics.Remove(entity);
        _updateables.Remove(entity);
    }

    public virtual void Update(double elapsed)
    {
        foreach (var updateable in _updateables.ToList())
        {
            updateable.Update(elapsed);
        }
    }

    public virtual void Draw()
    {
        foreach (var graphic in _graphics.ToList())
        {
            graphic.Render();
        }
    }
}
