using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

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
    public void Add(params IAddable[] children) => _container.Add(children);

    /// <inheritdoc/>
    public void Add(IAddable entity) => _container.Add(entity);

    /// <inheritdoc/>
    public void Remove(IAddable entity) => _container.Remove(entity);

    /// <inheritdoc/>
    public void RemoveAll() => _container.RemoveAll();

    /// <summary>For the world (the root container), local coordinates are window coordinates — returns the input unchanged.</summary>
    public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);

    /// <inheritdoc/>
    /// <remarks>
    /// Updates the main container fully (regular pass then <see cref="IUpdateable.UpdateLast"/>
    /// post-pass) before doing the same for the overlay. The overlay completes its own two
    /// passes self-contained — a collision solver in the main world will not see overlay
    /// entity positions until the next frame, and vice versa. In practice the overlay is for
    /// UI chrome (tooltips, modals) that should not be interacting with gameplay colliders.
    /// </remarks>
    public virtual void Update(double elapsed)
    {
        TickAll(_container, elapsed);
        TickAll(_overlay, elapsed);
    }

    private static void TickAll(Container container, double elapsed)
    {
        foreach (var updateable in container.GetUpdatables())
        {
            if (updateable.Active)
            {
                updateable.Update(elapsed);
            }
        }
        foreach (var updateable in container.GetUpdatablesLast())
        {
            if (updateable.Active)
            {
                updateable.Update(elapsed);
            }
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
