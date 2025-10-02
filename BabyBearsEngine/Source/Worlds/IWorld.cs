using BabyBearsEngine.Source.Graphics;

namespace BabyBearsEngine.Source.Worlds;
public interface IWorld
{
    void AddEntity(IEntity entity);
    void AddGraphic(IRenderable graphic);
    void DrawGraphics();
    void RemoveGraphic(IRenderable graphic);
    void Unload();
    void UpdateThings();
}