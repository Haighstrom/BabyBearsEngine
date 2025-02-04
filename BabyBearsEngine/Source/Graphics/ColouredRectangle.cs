using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics;

public class ColouredRectangle(ShaderProgramLibrary shaderLibrary, Color4 colour, float x, float y, float width, float height) : IGraphic, IDisposable
{
    private bool _disposed;

    private readonly SolidColourShaderProgram _shaderProgram = shaderLibrary.SolidColourShaderProgram;
    private readonly VertexDataBuffer<VertexNoTexture> _shaderConnector = new();
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

    private void UpdateVertices()
    {
        VertexNoTexture[] vertices =
        [
            new(x + width, y + height, Colour), // top right
            new(x + width, y, Colour), // bottom right
            new(x, y + height, Colour), // top left
            new(x, y, Colour), // bottom left
        ];

        _shaderConnector.SetNewVertices(vertices);
    }

    public void Draw()
    {
        _shaderProgram.Bind();
        _shaderConnector.Bind();

        if (_verticesChanged)
        {
            UpdateVertices();

            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
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
    #endregion
}
