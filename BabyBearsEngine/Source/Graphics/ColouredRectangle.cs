using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class ColouredRectangle(Color4 colour, float x, float y, float width, float height) : IRenderable, IDisposable
{
    private bool _disposed;

    private readonly SolidColourShaderProgram _shader = SolidColourShaderProgram.Instance;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();
    private bool _verticesChanged = true;

    public float X
    {
        get => x;
        set
        {
            x = value;
            _verticesChanged = true;
        }
    }

    public float Y
    {
        get => y;
        set
        {
            y = value;
            _verticesChanged = true;
        }
    }

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

    public Color4 Colour
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
            return
            [
                new(x + width, y + height, Colour), // top right
                new(x + width, y, Colour), // bottom right
                new(x, y + height, Colour), // top left
                new(x, y, Colour), // bottom left
            ];

        }
    }

    public void Render()
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();

        if (_verticesChanged)
        {
            _vertexDataBuffer.SetNewVertices(Vertices);
            _verticesChanged = false;
        }

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
