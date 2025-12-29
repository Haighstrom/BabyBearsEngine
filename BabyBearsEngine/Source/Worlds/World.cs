using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Source.Graphics;

namespace BabyBearsEngine.Source.Worlds;

public class World : IWorld
{
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    public void AddGraphic(IRenderable graphic)
    {
        _graphics.Add(graphic);
    }

    //public void AddUpdateable(IUpdateable updateable)
    //{
    //    _updateables.Add(updateable);
    //}

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
        foreach (var updateable in _updateables.ToList())
        {
            updateable.Update();
        }

        if (_image == null)
            _image = new("Assets/bear2.png", 200, 200, 100, 100);
    }

    Image _image;

    public World()
    {
    }

    public void DrawGraphics()
    {
        foreach (var graphic in _graphics.ToList())
        {
            graphic.Render();
        }

        _image.Render();
    }

    public void Unload()
    {
        foreach (var graphic in _graphics.ToList())
        {
            graphic.Dispose();
        }
    }
}
