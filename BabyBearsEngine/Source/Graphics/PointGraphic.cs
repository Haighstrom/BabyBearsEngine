using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class PointGraphic : IRenderable
{
    private bool _disposed;
    private readonly PointShaderProgram _shader;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();

    public PointGraphic(int x, int y, float size, Color4 colour)
    {
        _shader = new PointShaderProgram();
        _shader.SetPointSize(size);

        VertexNoTexture[] vertices =
        [
            new(x, y, colour),
        ];

        _vertexDataBuffer.SetNewVertices(vertices);
    }

    public void Render()
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();

        GL.DrawArrays(PrimitiveType.Points, 0, 1);
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _vertexDataBuffer.Dispose();
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
    #endregion
}
