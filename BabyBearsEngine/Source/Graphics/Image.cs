using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class Image : IDisposable
{
    private readonly Vertex[] _vertices =
    {
        new(0.5f, 0.5f, Color4.White, 1, 1), // top right
        new(0.5f, -0.5f, Color4.White, 1, 0), // bottom right
        new(-0.5f, 0.5f, Color4.White, 0, 1), // top left
        new(-0.5f, -0.5f, Color4.White, 0, 0), // bottom left
    };

    private readonly uint[] _indices =
    [
        0, 1, 3,   // first triangle
        0, 2, 3    // second triangle
    ];

    private readonly VBO _vBO;
    private readonly VAO _vAO;
    private readonly EBO _eBO;
    private readonly Shader _shader;
    private readonly Texture _texture;

    public Image(string texturePath)
    {
        _vAO = new VAO();
        _vAO.Bind();

        _vBO = new VBO();
        _vBO.Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * Vertex.Stride, _vertices, BufferUsageHint.StaticDraw);

        _eBO = new EBO();
        _eBO.Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        _shader = new Shader("shader.vert", "shader.frag");
        _shader.Bind();

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 2 * sizeof(float));

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 6 * sizeof(float));

        _texture = new Texture(texturePath);
        _texture.Bind(TextureUnit.Texture0);
    }

    public void Draw()
    {
        _vAO.Bind();
        _texture.Bind(TextureUnit.Texture0);
        _shader.Bind();
        //GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    #region IDisposable
    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _shader.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Triangle()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
