using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Default <see cref="IWorld"/> implementation. Subclass to build a custom scene/level/screen.
/// Override <see cref="Load"/> to set up entities and graphics; override <see cref="Update"/> for
/// custom per-frame logic. <see cref="Draw"/> handles screen clearing and renders all child
/// renderables; override only when you need bespoke render orchestration.
/// </summary>
public class World : IWorld
{
    private readonly Container _container;
    private readonly Container _overlay;

    /// <summary>Creates an empty world with the default background colour (cornflower blue).</summary>
    public World()
    {
        _container = new Container(this);
        _overlay = new Container(this);
    }

    /// <summary>The colour used to clear the screen at the start of each frame. Defaults to <see cref="Colour.CornflowerBlue"/>.</summary>
    public Colour BackgroundColour { get; set; } = Colour.CornflowerBlue;

    /// <inheritdoc/>
    public IContainer Overlay => _overlay;

    /// <inheritdoc/>
    public void Load()
    {
    }

    /// <inheritdoc/>
    public void Unload()
    {
        //foreach (var graphic in _graphics.ToList())
        //{
        //    graphic.Dispose();
        //}
    }

    /// <inheritdoc/>
    public void Add(IAddable entity) => _container.Add(entity);

    /// <inheritdoc/>
    public void Remove(IAddable entity) => _container.Remove(entity);

    /// <inheritdoc/>
    public void RemoveAll() => _container.RemoveAll();

    /// <summary>For the world (the root container), local coordinates are window coordinates — returns the input unchanged.</summary>
    public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);

    /// <inheritdoc/>
    public virtual void Update(double elapsed)
    {
        foreach (var updateable in _container.GetUpdatables())
        {
            if (!updateable.Active)
            {
                continue;
            }
            updateable.Update(elapsed);
        }
        foreach (var updateable in _overlay.GetUpdatables())
        {
            if (!updateable.Active)
            {
                continue;
            }
            updateable.Update(elapsed);
        }
    }

    /// <inheritdoc/>
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

        foreach (var graphic in _overlay.GetRenderables())
        {
            if (!graphic.Visible)
            {
                continue;
            }
            graphic.Render(ref projection, ref modelView);
        }
    }
}
