using BabyBearsEngine.Graphics;
using BabyBearsEngine.OpenGL;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

public class StbTrueTypeTextGraphic(float x, float y, float width, float height, string text) : IRenderable, IDisposable
{
    private readonly R8ChannelShaderProgram _shader = new();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly Texture _texture = FontTexture.GetStbFontTexture("Assets/Fonts/Times.ttf", text);

    private Color4 _colour = Color4.White;
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
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    private Vertex[] Vertices
    {
        get
        {
            return
            [
                new(x + width, y + height, Colour, 1, 1), // top right
                new(x + width, y, Colour, 1, 0), // bottom right
                new(x, y + height, Colour, 0, 1), // top left
                new(x, y, Colour, 0, 0), // bottom left
            ];
        }
    }

    public void Render(Source.Geometry.Matrix3 projection)
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();
        _texture.Bind();

        if (_verticesChanged)
        {
            _vertexDataBuffer.SetNewVertices(Vertices);

            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _vertexDataBuffer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
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
