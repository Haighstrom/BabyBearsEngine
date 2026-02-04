using System.Drawing;
using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

public interface IWorld
{
    void Load();
    void Unload();
    void Clear();
    void Add(IRenderable graphic);
    void Add(IUpdateable updateable);
    void Add(IEntity entity);
    void Remove(IRenderable graphic);
    void Remove(IUpdateable updateable);
    void Remove(IEntity entity);
    void Update(double elapsed);
    void Draw();
}
