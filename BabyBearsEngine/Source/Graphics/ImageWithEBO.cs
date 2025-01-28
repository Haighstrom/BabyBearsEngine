using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class Image : IGraphic, IDisposable
{
    private readonly Vertex[] _vertices;

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

    public Image(string texturePath, float x, float y, float width, float height)
    {
        _vertices =
        [
            new(x + width, y + height, Color4.White, 1, 1), // top right
            new(x + width, y, Color4.White, 1, 0), // bottom right
            new(x, y + height, Color4.White, 0, 1), // top left
            new(x, y, Color4.White, 0, 0), // bottom left
        ];

        _vAO = new VAO();
        _vBO = new VBO(BufferUsageHint.StaticDraw);

        _vAO.Use();
        _vBO.Use();

        VAO.DefineStandardVertexFormats();
        _vBO.BufferData(_vertices);

        _eBO = new EBO(BufferUsageHint.StaticDraw);

        _eBO.Use();
        _eBO.BufferData(_indices);

        _shader = new Shader("shader.vert", "shader.frag");

        _texture = new Texture(texturePath);
    }

    public void Draw(int windowWidth, int windowHeight)
    {
        _vAO.Use();
        _vBO.Use();
        _texture.Use();
        _shader.Use();

        var windowSizeLocation = GL.GetUniformLocation(_shader.Handle, "WindowSize");
        GL.Uniform2(windowSizeLocation, new Vector2(windowWidth, windowHeight));

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
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
    // ~Image()
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
