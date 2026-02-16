using System.Collections.Generic;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Buffers;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

public class TextImage : IRenderable, IDisposable
{
    private readonly StandardMatrixShaderProgram _shader = new();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly ITexture _texture;
    private readonly GeneratedFontStruct _fontStruct;
    private bool _disposedValue;
    private float _x;
    private float _y;
    private float _width;
    private float _height;
    private string _textToDisplay;
    private Colour _colour;
    private bool _verticesChanged = true;
    private float _extraSpaceWidth = 0;
    private float _extraLineSpacing = 0;

    public TextImage(FontDefinition fontDef, string textToDisplay, Colour colour, float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _textToDisplay = textToDisplay;
        _colour = colour;

        var font = new FontLoader().LoadFont(fontDef);

        _fontStruct = new FontBitmapGenerator().GenerateCharSpritesheetAndPositions(font, fontDef.CharactersToLoad, fontDef.AntiAliased, 13);

        _texture = new TextureFactory().GenTexture(_fontStruct.CharacterSS);
    }

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

    public string Text
    {
        get => _textToDisplay;
        set
        {
            _textToDisplay = value;
            _verticesChanged = true;
        }
    }

    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    private Vertex[] Vertices { get; set; } = [];

    public float ScaleX { get; set; } = 1;
    public float ScaleY { get; set; } = 1;
    public HAlignment HAlignment { get; set; } = HAlignment.Centred;
    public VAlignment VAlignment { get; set; } = VAlignment.Centred;

    private void SetVerticesSimple()
    {
        //foreach (var sg in _vertGroups)
        //{
        //    sg.Dispose();
        //}

        //_vertGroups.Clear();

        List<Vertex> vertices = [];

        var colorTK = Colour.ToOpenTK();

        var len = ScaleX * _fontStruct.MeasureString(_textToDisplay).X;

        float h = ScaleY * _fontStruct.HighestChar;

        float x = HAlignment switch //todo: was 'int'ed before to avoid looking shit with no AA - but buggers up text in cameras - complex if statement?
        {
            HAlignment.Left or HAlignment.Full => 0,
            HAlignment.Centred => (Width - len) / 2,
            HAlignment.Right => Width - len,
            _ => throw new Exception($"HText/SetVertices: alignment {HAlignment} was not catered for."),
        };
        float y = VAlignment switch //todo: was 'int'ed before to avoid looking shit with no AA - but buggers up text in cameras - complex if statement?
        {
            VAlignment.Top or VAlignment.Full => 0,
            VAlignment.Centred => (Height - h) / 2,
            VAlignment.Bottom => Height - h,
            _ => throw new Exception($"HText/SetVertices: alignment {VAlignment} was not catered for."),
        };
        //if (Underline)
        //    _linesToDraw.Add(new Line(Colour, UnderlineThickness, true, dest.BottomLeft.Shift(0, UnderlineOffset), dest.BottomLeft.Shift(len, UnderlineOffset)));
        //if (Strikethrough)
        //    _linesToDraw.Add(new Line(Colour, StrikethroughThickness, true, dest.CentreLeft.Shift(0, StrikethroughOffset), dest.CentreLeft.Shift(len, StrikethroughOffset)));
        
        foreach (char c in _textToDisplay)
        {
            var source = _fontStruct.CharPositionsNormalised[c];
            var w = _fontStruct.CharPositions[c].Size.X * ScaleX;

            vertices.Add(
                GeometryHelper.QuadToTris(
                    new Vertex(X + x, Y + y, colorTK, source.Min.X, source.Min.Y),
                    new Vertex(X + x + w, Y + y, colorTK, source.Max.X, source.Min.Y),
                    new Vertex(X + x, Y + y + h, colorTK, source.Min.X, source.Max.Y),
                    new Vertex(X + x + w, Y + y + h, colorTK, source.Max.X, source.Max.Y)
            ));

            x += w;

            if (c == ' ')
            {
                x += _extraSpaceWidth;
            }
            else
            {
                x += _extraLineSpacing;
            }
        }

        Vertices = vertices.ToArray();
    }

    public void Render(Matrix3 projection)
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();
        _texture.Bind();

        if (_shader is IWorldShader worldShader)
        {
            worldShader.SetProjectionMatrix(projection);
        }

        if (_verticesChanged)
        {
            SetVerticesSimple();

            _vertexDataBuffer.SetNewVertices(Vertices);

            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
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
