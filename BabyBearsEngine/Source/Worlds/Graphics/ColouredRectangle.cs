using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Graphics;

public class ColouredRectangle(Colour colour, float x, float y, float width, float height) : AddableBase, IRenderable, IDisposable
{
    private bool _disposed;

    private readonly SolidColourShaderProgramMatrix _shader = SolidColourShaderProgramMatrix.Instance;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();
    private bool _verticesChanged = true;

    public float X { get; set; } = x;
    public float Y { get; set; } = y;

    // Properties
    public bool Visible { get; set; } = true;

    public float Width
    {
        get => width;
        set
        {
            width = value;
            _verticesChanged = true;
        }
    }

    public float Height
    {
        get => height;
        set
        {
            height = value; 
            _verticesChanged = true;
        }
    }

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

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
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
