using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics;

namespace BabyBearsEngine.Source.Worlds;

public class World()
{
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    public void AddGraphic(IRenderable graphic)
    {
        _graphics.Add(graphic);
    }

    public void RemoveGraphic(IRenderable graphic)
    {
        _graphics.Remove(graphic);
    }

    public void AddEntity(IEntity entity)
    {
        _graphics.Add(entity);
        _updateables.Add(entity);
    }

    public void UpdateThings()
    {
        foreach (var updateable in _updateables)
        {
            updateable.Update();
        }
    }

    public void DrawGraphics()
    {
        foreach (var graphic in _graphics)
        {
            graphic.Draw();
        }
    }

    public void Unload()
    {
        foreach (var graphic in _graphics)
        {
            graphic.Dispose();
        }
    }
}
