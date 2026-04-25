using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Worlds;

public class World : IWorld
{
    private readonly Container _container;

    public World()
    {
        _container = new Container(this);
    }

    public Colour BackgroundColour { get; set; } = Colour.CornflowerBlue;

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


    public void Add(IAddable entity) => _container.Add(entity);

    public void Remove(IAddable entity) => _container.Remove(entity);

    public void RemoveAll() => _container.RemoveAll();
    public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);

    public virtual void Update(double elapsed)
    {
        foreach (var updateable in _container.GetUpdatables())
        {
            updateable.Update(elapsed);
        }
    }

    public virtual void Draw()
    {
        GL.ClearColor(BackgroundColour.R / 255f, BackgroundColour.G / 255f, BackgroundColour.B / 255f, BackgroundColour.A / 255f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var projection = Matrix3.CreateOrtho(Window.Width, Window.Height);
        var modelView = Matrix3.Identity;

        foreach (var graphic in _container.GetRenderables())
        {
            if (!graphic.Visible)
            {
                continue;
            }
            graphic.Render(ref projection, ref modelView);
        }
    }
}
