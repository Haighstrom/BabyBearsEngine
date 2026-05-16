using System.Collections.Generic;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Buffers;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public sealed class TextGraphic : GraphicBase, IGraphic, IDisposable
{
    private readonly StandardMatrixShaderProgram _shader = new();
    private readonly SolidColourShaderProgramMatrix _decorationShader = SolidColourShaderProgramMatrix.Instance;
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly VertexDataBuffer<VertexNoTexture> _decorationVertexDataBuffer = new();
    private ITexture _texture;
    private GeneratedFontStruct _fontStruct;
    private bool _disposedValue;
    private FontDefinition _fontDef;
    private string _textToDisplay;
    private Colour _colour;
    private bool _multiline = false;
    private TextDecoration? _strikethrough = null;
    private TextDecoration? _underline = null;
    private bool _verticesChanged = true;
    private float _extraCharSpacing = 0f;
    private float _extraLineSpacing = 0f;
    private float _extraSpaceWidth = 0f;

    public TextGraphic(FontDefinition fontDef, string textToDisplay, Colour colour, float x, float y, float width, float height)
        : base(x, y, width, height)
    {
        _textToDisplay = textToDisplay;
        _colour = colour;

        CachedFontAtlas atlas = FontTextureCache.GetOrCreate(fontDef);
        _fontDef = fontDef;
        _fontStruct = atlas.FontStruct;
        _texture = atlas.Texture;
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

    public float ExtraCharacterSpacing
    {
        get => _extraCharSpacing;
        set
        {
            _extraCharSpacing = value;
            _verticesChanged = true;
        }
    }

    public float ExtraLineSpacing
    {
        get => _extraLineSpacing;
        set
        {
            _extraLineSpacing = value;
            _verticesChanged = true;
        }
    }

    public float ExtraSpaceWidth
    {
        get => _extraSpaceWidth;
        set
        {
            _extraSpaceWidth = value;
            _verticesChanged = true;
        }
    }

    public FontDefinition Font
    {
        get => _fontDef;
        set
        {
            CachedFontAtlas atlas = FontTextureCache.GetOrCreate(value);
            _fontDef = value;
            _fontStruct = atlas.FontStruct;
            _texture = atlas.Texture;
            _verticesChanged = true;
        }
    }

    public bool Multiline
    {
        get => _multiline;
        set
        {
            _multiline = value;
            _verticesChanged = true;
        }
    }

    public TextDecoration? Strikethrough
    {
        get => _strikethrough;
        set
        {
            _strikethrough = value;
            _verticesChanged = true;
        }
    }

    public TextDecoration? Underline
    {
        get => _underline;
        set
        {
            _underline = value;
            _verticesChanged = true;
        }
    }

    private VertexNoTexture[] DecorationVertices { get; set; } = [];
    private Vertex[] Vertices { get; set; } = [];

    public float ScaleX { get; set; } = 1;
    public float ScaleY { get; set; } = 1;
    public HAlignment HAlignment { get; set; } = HAlignment.Centred;
    public VAlignment VAlignment { get; set; } = VAlignment.Centred;

    public Point MeasureString(string text)
    {
        var raw = _fontStruct.MeasureString(text);
        return new Point(raw.X * ScaleX, raw.Y * ScaleY);
    }

    public Point MeasureString()
    {
        if (!_multiline)
        {
            return MeasureString(_textToDisplay);
        }

        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(_textToDisplay, _fontStruct, Width, ScaleX, _extraSpaceWidth, _extraCharSpacing);
        float maxLineWidth = 0f;

        foreach (LineInfo line in lines)
        {
            float lw = TextLayout.MeasureLine(line.Content, _fontStruct, ScaleX, _extraSpaceWidth, _extraCharSpacing);
            if (lw > maxLineWidth)
            {
                maxLineWidth = lw;
            }
        }

        return new Point(maxLineWidth, lines.Count * (_fontStruct.HighestChar * ScaleY + _extraLineSpacing));
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _verticesChanged = true;
    }

    private IReadOnlyList<LineInfo> GetLines()
    {
        if (_multiline)
        {
            return TextLayout.ComputeLines(_textToDisplay, _fontStruct, Width, ScaleX, _extraSpaceWidth, _extraCharSpacing);
        }

        return [new LineInfo(_textToDisplay, 0, _textToDisplay.Length)];
    }

    private void SetVertices()
    {
        List<Vertex> vertices = [];
        List<VertexNoTexture> decorationVertices = [];

        var colorTK = Colour.ToOpenTK();
        float lineHeight = ScaleY * _fontStruct.HighestChar;
        IReadOnlyList<LineInfo> lines = GetLines();
        float totalHeight = lines.Count * (lineHeight + _extraLineSpacing);

        float y = MathF.Round(VAlignment switch
        {
            VAlignment.Top or VAlignment.Full => 0,
            VAlignment.Centred => (Height - totalHeight) / 2,
            VAlignment.Bottom => Height - totalHeight,
            _ => throw new Exception($"TextGraphic/SetVerticesSimple: VAlignment {VAlignment} was not catered for."),
        });

        foreach (LineInfo line in lines)
        {
            float lineWidth = TextLayout.MeasureLine(line.Content, _fontStruct, ScaleX, _extraSpaceWidth, _extraCharSpacing);

            float x = MathF.Round(HAlignment switch
            {
                HAlignment.Left or HAlignment.Full => 0,
                HAlignment.Centred => (Width - lineWidth) / 2,
                HAlignment.Right => Width - lineWidth,
                _ => throw new Exception($"TextGraphic/SetVerticesSimple: HAlignment {HAlignment} was not catered for."),
            });

            float xStart = x;

            foreach (char c in line.Content)
            {
                var source = _fontStruct.GetCharPositionNormalised(c);
                float w = _fontStruct.GetCharPosition(c).Size.X * ScaleX;

                vertices.Add(
                    GeometryHelper.QuadToTris(
                        new Vertex(x,     y,              colorTK, source.Min.X, source.Min.Y),
                        new Vertex(x + w, y,              colorTK, source.Max.X, source.Min.Y),
                        new Vertex(x,     y + lineHeight, colorTK, source.Min.X, source.Max.Y),
                        new Vertex(x + w, y + lineHeight, colorTK, source.Max.X, source.Max.Y)
                    ));

                x += w;

                if (c == ' ')
                {
                    x += _extraSpaceWidth;
                }
                else
                {
                    x += _extraCharSpacing;
                }
            }

            if (_underline is TextDecoration ul)
            {
                decorationVertices.Add(DecorationQuad(xStart, y + lineHeight, lineWidth, ul.Thickness, ul.Colour.ToOpenTK()));
            }

            if (_strikethrough is TextDecoration st)
            {
                float stY = y + (lineHeight - st.Thickness) / 2f;
                decorationVertices.Add(DecorationQuad(xStart, stY, lineWidth, st.Thickness, st.Colour.ToOpenTK()));
            }

            y += lineHeight + _extraLineSpacing;
        }

        Vertices = vertices.ToArray();
        DecorationVertices = decorationVertices.ToArray();
    }

    private static VertexNoTexture[] DecorationQuad(float x, float y, float width, float height, OpenTK.Mathematics.Color4 colour)
    {
        return
        [
            new(x,         y,          colour),
            new(x + width, y,          colour),
            new(x,         y + height, colour),
            new(x + width, y,          colour),
            new(x,         y + height, colour),
            new(x + width, y + height, colour),
        ];
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (_verticesChanged)
        {
            SetVertices();
            _vertexDataBuffer.SetNewVertices(Vertices);
            _decorationVertexDataBuffer.SetNewVertices(DecorationVertices);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        _shader.Bind();
        _vertexDataBuffer.Bind();
        _texture.Bind();
        if (_shader is IMVPShader mvpShader)
        {
            mvpShader.SetProjectionMatrix(ref projection);
            mvpShader.SetModelViewMatrix(ref mv);
        }
        GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);

        if (DecorationVertices.Length > 0)
        {
            _decorationShader.Bind();
            _decorationVertexDataBuffer.Bind();
            _decorationShader.SetProjectionMatrix(ref projection);
            _decorationShader.SetModelViewMatrix(ref mv);
            GL.DrawArrays(PrimitiveType.Triangles, 0, DecorationVertices.Length);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _vertexDataBuffer.Dispose();
                _decorationVertexDataBuffer.Dispose();
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
