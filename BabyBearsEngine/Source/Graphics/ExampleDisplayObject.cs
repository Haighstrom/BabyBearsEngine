using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class ExampleDisplayObject(IShader shader, int width, int height, ITexture texture) 
    : IRenderable
{
    private readonly IShader _shader = shader;
    private readonly ITexture _texture = texture;
    private readonly int _vertexBuffer = GL.GenBuffer();
    private readonly Vertex[] _vertices =
    [
        new(0, 0, Color4.White, 0, 0),
        new(width, 0, Color4.White, 1, 0),
        new(0, height, Color4.White, 0, 1),
        new(width, height, Color4.White, 1, 1)
    ];

    public void Render(int x, int y)
    {
        throw new NotImplementedException();
    }
}
