using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Graphics;

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
public sealed class ColouredRectangle(Colour colour, float x, float y, float width, float height) : GraphicBase, IGraphic, IDisposable
{
    private bool _disposed;

    private readonly SolidColourShaderProgramMatrix _shader = SolidColourShaderProgramMatrix.Instance;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();
    private bool _verticesChanged = true;

    /// <summary>X position in the parent's local space.</summary>
    public float X { get; set; } = x;

    /// <summary>Y position in the parent's local space.</summary>
    public float Y { get; set; } = y;

    /// <summary>Width in pixels.</summary>
    public float Width
    {
        get => width;
        set
        {
            width = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Height in pixels.</summary>
    public float Height
    {
        get => height;
        set
        {
            height = value;
            _verticesChanged = true;
        }
    }

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
            var colorTK = Colour.ToOpenTK();

            return
            [
                new(width, height, colorTK), // top right
                new(width, 0,      colorTK), // bottom right
                new(0,     height, colorTK), // top left
                new(0,     0,      colorTK), // bottom left
            ];
        }
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
                // TODO: dispose managed state (managed objects)
                _vertexDataBuffer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
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
}
