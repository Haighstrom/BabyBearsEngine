using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Graphics;

public interface IRenderable
{
    void Render(ref Matrix3 projection, ref Matrix3 modelView);
}
