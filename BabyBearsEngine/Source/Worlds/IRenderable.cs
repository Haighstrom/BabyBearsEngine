using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Graphics;

public interface IRenderable : IAddable
{
    void Render(ref Matrix3 projection, ref Matrix3 modelView);

    // Whether this renderable should be drawn. Default implementations
    // will initialise this to true on concrete types.
    bool Visible { get; set; }
}
