using System.Security.Cryptography;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class ImageWithEBO : IGraphic, IDisposable
{
    private readonly uint[] _indices =
    [
        0, 1, 3,   // first triangle
        0, 2, 3    // second triangle
    ];

    private readonly VBO _vBO;
    private readonly VAO _vAO;
    private readonly EBO _eBO;
    private readonly Texture _texture;
    private readonly float _x;
    private readonly float _y;
    private readonly float _width;
    private readonly float _height;

    public ImageWithEBO(string texturePath, float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;

        _vAO = new VAO();
        _vBO = new VBO(BufferUsageHint.StaticDraw);

        _vAO.Bind();
        _vBO.Bind();

        _eBO = new EBO(BufferUsageHint.StaticDraw);

        _eBO.Bind();
        _eBO.BufferData(_indices);

        _texture = new Texture(texturePath);
    }

    public void Draw()
    {
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
