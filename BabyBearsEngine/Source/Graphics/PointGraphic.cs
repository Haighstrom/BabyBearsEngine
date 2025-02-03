using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Graphics;

public class PointGraphic : IGraphic
{
    private bool _disposed;
    private readonly PointShader _shaderProgram;
    private readonly VAO _vAO;
    private readonly VBO _vBO;

    public PointGraphic(GameWindow window, int x, int y, float size, Color4 colour)
    {
        _shaderProgram = new PointShader(window);
        _shaderProgram.SetPointSize(size);

        //Create and bind the VAO (which will store the VBO binding)
        _vAO = new VAO();
        _vAO.Bind();

        //Create and bind the VBO (which contains the objects vertices)
        _vBO = new VBO(BufferUsageHint.StaticDraw);
        _vBO.Bind();

        // Define attributes (this links the VBO to the VAO)
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 2 * sizeof(float));

        _vBO.Bind();

        Vertex[] vertices =
        [
            new(x, y, colour, 0, 0),
        ];

        _vBO.BufferData(vertices);
    }

    public void Draw()
    {
        _shaderProgram.Bind();
        _vAO.Bind();

        GL.DrawArrays(PrimitiveType.Points, 0, 1);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PointGraphic()
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
}
