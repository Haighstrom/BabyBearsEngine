using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

public interface IWorld
{
    void Load();
    void AddEntity(IEntity entity);
    void AddGraphic(IRenderable graphic);
    void Clear();
    void DrawGraphics();
    void RemoveGraphic(IRenderable graphic);
    void Unload();
    void UpdateThings(double elapsed);
}
