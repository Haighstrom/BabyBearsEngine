using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public sealed class StbTrueTypeTextGraphic(float x, float y, float width, float height, string text, int layer = int.MaxValue) 
    : GraphicBase(x, y, width, height, layer), IDisposable
{
    private readonly R8ChannelShaderProgram _shader = new();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly Texture _texture = FontTexture.GetStbFontTexture("Assets/Fonts/Times.ttf", text);

    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;

    public Colour Colour
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
            var c = _colour.ToOpenTK();
            return
            [
                new(X + Width, Y + Height, c, 1, 1), // top right
                new(X + Width, Y,          c, 1, 0), // bottom right
                new(X,         Y + Height, c, 0, 1), // top left
                new(X,         Y,          c, 0, 0), // bottom left
            ];
        }
    }

    protected override void OnPositionChanged()
    {
        base.OnPositionChanged();
        _verticesChanged = true;
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
        _texture.Bind();

        if (_verticesChanged)
        {
            _vertexDataBuffer.SetNewVertices(Vertices);

            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    private bool _disposedValue;

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _vertexDataBuffer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
