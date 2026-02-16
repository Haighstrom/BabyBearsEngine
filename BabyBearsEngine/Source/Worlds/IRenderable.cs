using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Graphics;

public interface IRenderable : IDisposable
{
    void Render(Matrix3 projection);
}
