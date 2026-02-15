using BabyBearsEngine.Graphics;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Worlds;

public interface IContainer
{
    void Clear();
    void Add(IRenderable graphic);
    void Add(IUpdateable updateable);
    void Add(IEntity entity);
    void Remove(IRenderable graphic);
    void Remove(IUpdateable updateable);
    void Remove(IEntity entity);
}
