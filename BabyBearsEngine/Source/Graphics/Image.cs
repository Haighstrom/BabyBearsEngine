using System.Threading.Tasks.Dataflow;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Graphics.Textures;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

public class Image : IRenderable, IDisposable
{
    public IShaderProgram Shader { get; set; } = new StandardMatrixShaderProgram();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly ITexture _texture;

    private Color4 _colour = Color4.White;
    private bool _verticesChanged = true;

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            _verticesChanged = true;
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            _verticesChanged = true;
        }
    }

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            _verticesChanged = true;
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value; 
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
                new(_x, _y, Colour, 0, 0), // bottom left
                new(_x + _width, _y, Colour, 1, 0), // bottom right
                new(_x, _y + _height, Colour, 0, 1), // top left
                new(_x + _width, _y + _height, Colour, 1, 1), // top right
            ];
        }
    }

    public void Render()
    {
        Shader.Bind();
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
    private float _x;
    private float _y;
    private float _width;
    private float _height;

    public Image(string texturePath, float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _texture = new TextureFactory().CreateTextureFromImageFile(texturePath);
    }

    public Image(ITexture texture, float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _texture = texture;
    }

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
