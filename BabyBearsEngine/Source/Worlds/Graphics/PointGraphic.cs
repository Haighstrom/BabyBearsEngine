using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Graphics;

/// <summary>
/// Renders a single coloured point at a fixed pixel size. Construction allocates GL resources
/// (shader, vertex buffer) — must be created on the engine thread after the GL context exists.
/// Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
public sealed class PointGraphic : GraphicBase
{
    private bool _disposed;
    private readonly PointShaderProgram _shader;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();

    /// <summary>Creates a point at (<paramref name="x"/>, <paramref name="y"/>) with the given pixel size and colour.</summary>
    /// <param name="x">X position in the parent's local space.</param>
    /// <param name="y">Y position in the parent's local space.</param>
    /// <param name="size">Point diameter in pixels.</param>
    /// <param name="colour">Point colour.</param>
    public PointGraphic(int x, int y, float size, Colour colour)
    {
        _shader = new PointShaderProgram();
        _shader.SetPointSize(size);

        VertexNoTexture[] vertices =
        [
            new(x, y, colour.ToOpenTK()),
        ];

        _vertexDataBuffer.SetNewVertices(vertices);
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();

        GL.DrawArrays(PrimitiveType.Points, 0, 1);
    }

    private void Dispose(bool disposing)
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
}
