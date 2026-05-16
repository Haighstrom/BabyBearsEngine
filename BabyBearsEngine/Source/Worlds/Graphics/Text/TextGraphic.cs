using System.Collections.Generic;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Buffers;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;

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
    private int _numCharsToDraw = int.MaxValue;
    private float _scaleX = 1f;
    private float _scaleY = 1f;
    private TextDecoration? _strikethrough = null;
    private TextDecoration? _underline = null;
    private bool _verticesChanged = true;
    private bool _wasTruncated = false;
    private float _extraCharSpacing = 0f;
    private float _extraLineSpacing = 0f;
    private float _extraSpaceWidth = 0f;
    private int _firstCharToDraw = 0;

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

    /// <inheritdoc/>
    public string Text
    {
        get => _textToDisplay;
        set
        {
            _textToDisplay = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public float ExtraCharacterSpacing
    {
        get => _extraCharSpacing;
        set
        {
            _extraCharSpacing = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public float ExtraLineSpacing
    {
        get => _extraLineSpacing;
        set
        {
            _extraLineSpacing = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public float ExtraSpaceWidth
    {
        get => _extraSpaceWidth;
        set
        {
            _extraSpaceWidth = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public int FirstCharToDraw
    {
        get => _firstCharToDraw;
        set
        {
            _firstCharToDraw = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool Multiline
    {
        get => _multiline;
        set
        {
            _multiline = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public int NumCharsToDraw
    {
        get => _numCharsToDraw;
        set
        {
            _numCharsToDraw = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public TextDecoration? Strikethrough
    {
        get => _strikethrough;
        set
        {
            _strikethrough = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public float ScaleX
    {
        get => _scaleX;
        set
        {
            _scaleX = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public float ScaleY
    {
        get => _scaleY;
        set
        {
            _scaleY = value;
            _verticesChanged = true;
        }
    }

    public HAlignment HAlignment { get; set; } = HAlignment.Centred;
    public VAlignment VAlignment { get; set; } = VAlignment.Centred;

    /// <inheritdoc/>
    public Point MeasureString(string text)
    {
        var raw = _fontStruct.MeasureString(text);
        return new Point(raw.X * ScaleX, raw.Y * ScaleY);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void ScaleForCamera(ICamera camera) => ScaleForCamera(camera.View);

    /// <inheritdoc/>
    public void ScaleForCamera(ICameraView view)
    {
        ScaleX = 1f / view.TileWidth;
        ScaleY = 1f / view.TileHeight;
    }

    private void SetVertices()
    {
        List<Vertex> vertices = [];
        List<VertexNoTexture> decorationVertices = [];

        var glColour = Colour.ToOpenTK();
        float lineHeight = ScaleY * _fontStruct.HighestChar;
        IReadOnlyList<LineInfo> lines = GetLines();
        float totalHeight = lines.Count * (lineHeight + _extraLineSpacing);

        float lineTop = MathF.Round(VAlignment switch
        {
            VAlignment.Top or VAlignment.Full => 0,
            VAlignment.Centred => (Height - totalHeight) / 2,
            VAlignment.Bottom => Height - totalHeight,
            _ => throw new Exception($"TextGraphic/SetVerticesSimple: VAlignment {VAlignment} was not catered for."),
        });

        int globalCharIndex = 0;
        // Cast to long: _firstCharToDraw + int.MaxValue would overflow int.
        long visibleEnd = (long)_firstCharToDraw + _numCharsToDraw;

        bool truncated = false;

        foreach (LineInfo line in lines)
        {
            // Lines beyond Height are cut off; flag truncation if visible chars remain.
            if (lineTop >= Height)
            {
                if (globalCharIndex < visibleEnd)
                {
                    truncated = true;
                }

                break;
            }

            float lineWidth = TextLayout.MeasureLine(line.Content, _fontStruct, ScaleX, _extraSpaceWidth, _extraCharSpacing);

            float charLeft = MathF.Round(HAlignment switch
            {
                HAlignment.Left or HAlignment.Full => 0,
                HAlignment.Centred => (Width - lineWidth) / 2,
                HAlignment.Right => Width - lineWidth,
                _ => throw new Exception($"TextGraphic/SetVerticesSimple: HAlignment {HAlignment} was not catered for."),
            });

            // Track the x-span of rendered characters on this line for decoration quads.
            float? decorationLeft = null;
            float decorationRight = 0f;

            foreach (char c in line.Content)
            {
                var atlasUV = _fontStruct.GetCharPositionNormalised(c);
                float glyphWidth = _fontStruct.GetCharPosition(c).Size.X * ScaleX;
                float charAdvance = glyphWidth + (c == ' ' ? _extraSpaceWidth : _extraCharSpacing);

                if (globalCharIndex >= _firstCharToDraw && globalCharIndex < visibleEnd)
                {
                    float quadLeft   = charLeft;
                    float quadTop    = lineTop;
                    float quadRight  = charLeft + glyphWidth;
                    float quadBottom = lineTop + lineHeight;

                    if (glyphWidth > 0 && quadRight > 0 && quadLeft < Width && quadBottom > 0 && quadTop < Height)
                    {
                        float uvLeft   = atlasUV.Min.X;
                        float uvTop    = atlasUV.Min.Y;
                        float uvRight  = atlasUV.Max.X;
                        float uvBottom = atlasUV.Max.Y;

                        // Clip the quad to graphic bounds, adjusting UVs proportionally to sample only the visible slice.
                        if (quadLeft < 0)
                        {
                            truncated = true;
                            uvLeft += (uvRight - uvLeft) * -quadLeft / (quadRight - quadLeft);
                            quadLeft = 0;
                        }
                        if (quadRight > Width)
                        {
                            truncated = true;
                            uvRight -= (uvRight - uvLeft) * (quadRight - Width) / (quadRight - quadLeft);
                            quadRight = Width;
                        }
                        if (quadTop < 0)
                        {
                            truncated = true;
                            uvTop += (uvBottom - uvTop) * -quadTop / (quadBottom - quadTop);
                            quadTop = 0;
                        }
                        if (quadBottom > Height)
                        {
                            truncated = true;
                            uvBottom -= (uvBottom - uvTop) * (quadBottom - Height) / (quadBottom - quadTop);
                            quadBottom = Height;
                        }

                        decorationLeft ??= quadLeft;
                        // Advance covers spacing after the glyph; decorations span the full advance, not just glyph width.
                        decorationRight = MathF.Min(charLeft + charAdvance, Width);

                        vertices.Add(GeometryHelper.QuadToTris(
                            new Vertex(quadLeft,  quadTop,    glColour, uvLeft,  uvTop),
                            new Vertex(quadRight, quadTop,    glColour, uvRight, uvTop),
                            new Vertex(quadLeft,  quadBottom, glColour, uvLeft,  uvBottom),
                            new Vertex(quadRight, quadBottom, glColour, uvRight, uvBottom)
                        ));
                    }
                    else if (glyphWidth > 0) // zero-width glyphs (pure spacing) don't count as truncation
                    {
                        truncated = true;
                    }
                }

                charLeft += charAdvance;
                globalCharIndex++;
            }

            if (decorationLeft.HasValue)
            {
                float decorationWidth = decorationRight - decorationLeft.Value;

                if (_underline is TextDecoration underline)
                {
                    float underlineTop    = lineTop + lineHeight;
                    float underlineBottom = underlineTop + underline.Thickness;
                    if (underlineTop < Height && underlineBottom > 0)
                    {
                        decorationVertices.Add(DecorationQuad(decorationLeft.Value, MathF.Max(underlineTop, 0f), decorationWidth, MathF.Min(underlineBottom, Height) - MathF.Max(underlineTop, 0f), underline.Colour.ToOpenTK()));
                    }
                }

                if (_strikethrough is TextDecoration strikethrough)
                {
                    float strikethroughTop    = lineTop + (lineHeight - strikethrough.Thickness) / 2f;
                    float strikethroughBottom = strikethroughTop + strikethrough.Thickness;
                    if (strikethroughTop < Height && strikethroughBottom > 0)
                    {
                        decorationVertices.Add(DecorationQuad(decorationLeft.Value, MathF.Max(strikethroughTop, 0f), decorationWidth, MathF.Min(strikethroughBottom, Height) - MathF.Max(strikethroughTop, 0f), strikethrough.Colour.ToOpenTK()));
                    }
                }
            }

            lineTop += lineHeight + _extraLineSpacing;
        }

        Vertices = vertices.ToArray();
        DecorationVertices = decorationVertices.ToArray();

        if (truncated && !_wasTruncated)
        {
            string preview = _textToDisplay.Length > 40 ? string.Concat(_textToDisplay.AsSpan(0, 40), "...") : _textToDisplay;
            Logger.Warning($"TextGraphic text is truncated ({Width}x{Height}px): \"{preview}\"");
        }
        _wasTruncated = truncated;
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
