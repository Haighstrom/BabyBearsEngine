using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A solid-colour filled rectangle. Construction allocates GL resources (vertex buffer, shader binding) —
/// must be created on the engine thread after the GL context exists. Implements <see cref="IDisposable"/>
/// to release those resources.
/// </summary>
/// <param name="colour">Fill colour.</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
public sealed class ColourGraphic(Colour colour, float x, float y, float width, float height, int layer = 0) : GraphicBase(x, y, width, height, layer), IGraphic, IColourGraphic, IDisposable
{
    private readonly SolidColourShaderProgramMatrix _shader = SolidColourShaderProgramMatrix.Instance;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <summary>Fill colour.</summary>
    public Colour Colour
    {
        get => colour;
        set
        {
            colour = value;
            _verticesChanged = true;
        }
    }

    private VertexNoTexture[] Vertices
    {
        get
        {
            var colorTK = colour.ToOpenTK();

            return
            [
                new(Width, Height, colorTK), // top right
                new(Width, 0,      colorTK), // bottom right
                new(0,     Height, colorTK), // top left
                new(0,     0,      colorTK), // bottom left
            ];
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _verticesChanged = true;
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();

        if (_verticesChanged)
        {
            _vertexDataBuffer.SetNewVertices(Vertices);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
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
