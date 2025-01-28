using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics;

namespace BabyBearsEngine.Source.Worlds;

public class World()
{
    private readonly List<IGraphic> _graphics = [];

    public void AddGraphic(IGraphic graphic)
    {
        _graphics.Add(graphic);
    }

    public void RemoveGraphic(IGraphic graphic)
    {
        _graphics.Remove(graphic);
    }

    public void DrawGraphics(int windowWidth, int windowHeight)
    {
        foreach (var graphic in _graphics)
        {
            graphic.Draw(windowWidth, windowHeight);
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
