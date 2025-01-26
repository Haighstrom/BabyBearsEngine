using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

public class Rectangle : IDisposable
{
    private readonly float[] _vertices = 
    [
         0.5f,  0.5f, 0.0f,  // top right
         0.5f, -0.5f, 0.0f,  // bottom right
        -0.5f, -0.5f, 0.0f,  // bottom left
        -0.5f,  0.5f, 0.0f   // top left
    ];

    private readonly uint[] _indices = 
    [
        0, 1, 3,   // first triangle
        1, 2, 3    // second triangle
    ];

    private readonly VBO _vBO;
    private readonly VAO _vAO;
    private readonly EBO _eBO;
    private readonly Shader _shader;
    private readonly System.Diagnostics.Stopwatch _timer = new();

    public Rectangle()
    {
        _shader = new Shader("shader.vert", "shader.frag");

        _vAO = new VAO();
        _vBO = new VBO();
        _eBO = new EBO();

        _vAO.Bind();
        _vBO.Bind();
        _eBO.Bind();

        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        GL.EnableVertexAttribArray(0);

        _timer.Start();
    }

    public void Draw()
    {
        _shader.Bind();

        double timeValue = _timer.Elapsed.TotalSeconds;
        float greenValue = (float)Math.Sin(timeValue) / 2.0f + 0.5f;
        int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");
        GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

        _vAO.Bind();
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
