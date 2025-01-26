using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class Triangle : IDisposable
{
    private readonly Vertex[] _vertices =
    {
        new(0.5f, -0.5f, Color4.Red, 0, 0), //Bottom-right vertex
        new(-0.5f, -0.5f, new Color4(0,255,0,255), 0, 0), //Bottom-left vertex
        new(0.0f,  0.5f, Color4.Blue, 0, 0), //Top vertex
    };

    private readonly VBO _vBO;
    private readonly VAO _vAO;
    private readonly Shader _shader;

    public Triangle()
    {
        _vBO = new VBO();
        _vAO = new VAO();
        _shader = new Shader("shader.vert", "shader.frag");

        _vAO.Bind();
        _vBO.Bind();

        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * Vertex.Stride, _vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    public void Draw()
    {
        _shader.Bind();
        _vAO.Bind();
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
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
