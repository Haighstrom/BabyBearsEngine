using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public interface IShader
{
    void Render(int x, int y, int w, int h, PrimitiveType drawType);
}
