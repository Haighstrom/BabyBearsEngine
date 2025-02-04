using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics;

public class PointGraphic : IGraphic
{
    private bool _disposed;
    private readonly PointShaderProgram _shaderProgram;
    private readonly VertexDataBuffer<VertexNoTexture> _shaderConnector = new();

    public PointGraphic(ShaderProgramLibrary shaderLibrary, int x, int y, float size, Color4 colour)
    {
        _shaderProgram = shaderLibrary.PointShaderProgram;
        _shaderProgram.SetPointSize(size);

        VertexNoTexture[] vertices =
        [
            new(x, y, colour),
        ];

        _shaderConnector.SetNewVertices(vertices);
    }

    public void Draw()
    {
        _shaderProgram.Bind();
        _shaderConnector.Bind();

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
                _shaderConnector.Dispose();
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
