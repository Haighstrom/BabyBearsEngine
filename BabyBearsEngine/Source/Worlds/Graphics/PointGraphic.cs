using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Renders a single coloured point at a fixed pixel size. Construction allocates GL resources
/// (shader, vertex buffer) — must be created on the engine thread after the GL context exists.
/// Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
/// <remarks>
/// <para>The point is drawn at (<see cref="IRect.X"/>, <see cref="IRect.Y"/>) — i.e. the
/// inherited position from <see cref="AddableRectBase"/>. <see cref="IRect.Width"/> and
/// <see cref="IRect.Height"/> are exposed for consistency with other <see cref="IGraphic"/>s
/// but are not used to compute the rendered size; the point's pixel size comes from the
/// <c>size</c> constructor parameter.</para>
/// </remarks>
public sealed class PointGraphic : GraphicBase, IGraphic, IDisposable
{
    private readonly PointShaderProgram _shader = new();
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();
    private Colour _colour;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <param name="x">X position in the parent's local space.</param>
    /// <param name="y">Y position in the parent's local space.</param>
    /// <param name="size">Point diameter in pixels.</param>
    /// <param name="colour">Point colour.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public PointGraphic(float x, float y, float size, Colour colour, int layer = int.MaxValue)
        : base(x, y, size, size, layer)
    {
        _colour = colour;
        _shader.SetPointSize(size);
    }

    /// <summary>Point colour.</summary>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    protected override void OnPositionChanged()
    {
        base.OnPositionChanged();
        _verticesChanged = true;
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();

        if (_verticesChanged)
        {
            _vertexDataBuffer.SetNewVertices([new(X, Y, _colour.ToOpenTK())]);
            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.Points, 0, 1);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _vertexDataBuffer.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
