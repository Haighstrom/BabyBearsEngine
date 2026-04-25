using System.Collections.Generic;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Source.Worlds;

public abstract class ContainerEntity : AddableBase, IEntity, IContainer
{
    private readonly Container _container;

    protected ContainerEntity()
    {
        _container = new Container(this);
    }

    public bool Active { get; set; } = true;
    public bool Visible { get; set; } = true;

    protected IList<IUpdateable> GetUpdatables() => _container.GetUpdatables();
    protected IList<IRenderable> GetRenderables() => _container.GetRenderables();

    public virtual void Update(double elapsed)
    {
        foreach (var entity in GetUpdatables())
        {
            if (!entity.Active)
            {
                continue;
            }

            entity.Update(elapsed);
        }
    }

    public virtual void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        foreach (var entity in GetRenderables())
        {
            if (!entity.Visible)
            {
                continue;
            }
            
            entity.Render(ref projection, ref modelView);
        }
    }

    public void Add(IAddable entity) => _container.Add(entity);
    public void Remove(IAddable entity) => _container.Remove(entity);
    public void RemoveAll() => _container.RemoveAll();
    public virtual (float x, float y) GetWindowCoordinates(float x, float y) =>
        Parent?.GetWindowCoordinates(x, y) ?? (x, y);
}
