using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Graphics;

public interface IRenderable : IAddable
{
    void Render(ref Matrix3 projection, ref Matrix3 modelView);
}
