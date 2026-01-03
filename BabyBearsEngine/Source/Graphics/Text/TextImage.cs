using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;
using BabyBearsEngine.Source.Graphics.Textures;
using BabyBearsEngine.Source.Tools;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics.Text;

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
    private readonly string _textToDisplay;
    private Color4 _colour;
    private bool _verticesChanged = true;

    public TextImage(FontDefinition fontDef, string textToDisplay, Color4 colour, float x, float y, float width, float height)
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

    public Color4 Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    private Vertex[] Vertices { get; set; } = [];

    float ScaleX = 1;
    float ScaleY = 1;
    HAlignment HAlignment = HAlignment.Left;
    VAlignment VAlignment = VAlignment.Top;
    private float _extraSpaceWidth = 0;
    private float _extraLineSpacing = 0;

    private void SetVerticesSimple()
    {
        //foreach (var sg in _vertGroups)
        //{
        //    sg.Dispose();
        //}

        //_vertGroups.Clear();

        List<Vertex> vertices = [];

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
        bool first = true;
        foreach (char c in _textToDisplay)
        {
            Box2 source = _fontStruct.CharPositionsNormalised[c];
            var w = _fontStruct.CharPositions[c].Size.X * ScaleX;

            vertices.Add(
                Geometry.QuadToTris(
            //new Vertex(_x, _y, Colour, source.Min.X, source.Min.Y), // bottom left
            //new Vertex(_x + _width, _y, Colour, source.Max.X, source.Min.Y), // bottom right
            //new Vertex(_x, _y + _height, Colour, source.Min.X, source.Max.Y), // top left
            //new Vertex(_x + _width, _y + _height, Colour, source.Max.X, source.Max.Y) // top right
            new Vertex(X + x, Y + y, Colour, source.Min.X, source.Min.Y),
            new Vertex(X + x + w, Y + y, Colour, source.Max.X, source.Min.Y),
            new Vertex(X + x, Y + y + h, Colour, source.Min.X, source.Max.Y),
            new Vertex(X + x + w, Y + y + h, Colour, source.Max.X, source.Max.Y)
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

    public void Render()
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();
        _texture.Bind();

        if (_verticesChanged)
        {
            SetVerticesSimple();

            _vertexDataBuffer.SetNewVertices(Vertices);

            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
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
