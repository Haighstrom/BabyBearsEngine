using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Buffers;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using BabyBearsEngine.Worlds.Cameras;
using BabyBearsEngine.Worlds.UI.Themes;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public sealed class TextGraphic : GraphicBase, IGraphic, ITextGraphic, IDisposable
{
    private readonly SolidColourShaderProgramMatrix _decorationShader = SolidColourShaderProgramMatrix.Instance;
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly VertexDataBuffer<VertexNoTexture> _decorationVertexDataBuffer = new();
    private IMatrixShaderProgram _shader;
    private ITexture _texture;
    private GeneratedFontStruct _fontStruct;
    private bool _disposedValue;
    private FontDefinition _fontDef;
    private string _textToDisplay;
    private Colour _colour;
    private float _angle = 0f;
    private bool _multiline = false;
    private int _numCharsToDraw = int.MaxValue;
    private float _scaleX = 1f;
    private float _scaleY = 1f;
    private TextDecoration? _strikethrough = null;
    private TextDecoration? _underline = null;
    private bool _useInlineTags = true;
    private bool _verticesChanged = true;
    private bool _wasTruncated = false;
    private float _extraCharSpacing = 0f;
    private float _extraLineSpacing = 0f;
    private float _extraSpaceWidth = 0f;
    private int _firstCharToDraw = 0;

    /// <param name="fontDef">Font to render with. Resolved through the font atlas cache.</param>
    /// <param name="textToDisplay">The text string to render.</param>
    /// <param name="colour">Glyph colour.</param>
    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public TextGraphic(FontDefinition fontDef, string textToDisplay, Colour colour, float x, float y, float width, float height, int layer = int.MaxValue)
        : base(x, y, width, height, layer)
    {
        _textToDisplay = NormalizeNewlines(textToDisplay);
        _colour = colour;

        CachedFontAtlas atlas = FontTextureCache.GetOrCreate(fontDef);
        _fontDef = fontDef;
        _fontStruct = atlas.FontStruct;
        _texture = atlas.Texture;
        _shader = atlas.Shader;
    }

    /// <param name="theme">Visual styling — font, colour, and alignment.</param>
    /// <param name="textToDisplay">The text string to render.</param>
    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public TextGraphic(TextTheme theme, string textToDisplay, float x, float y, float width, float height, int layer = int.MaxValue)
        : this(theme.Font, textToDisplay, theme.Colour, x, y, width, height, layer)
    {
        HAlignment = theme.HAlignment;
        VAlignment = theme.VAlignment;
    }

    /// <param name="theme">Visual styling — font, colour, and alignment.</param>
    /// <param name="textToDisplay">The text string to render.</param>
    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public TextGraphic(TextTheme theme, string textToDisplay, Rect rect, int layer = int.MaxValue)
        : this(theme, textToDisplay, rect.X, rect.Y, rect.W, rect.H, layer)
    {
    }

    /// <inheritdoc/>
    public float Angle
    {
        get => _angle;
        set => _angle = value;
    }

    /// <inheritdoc/>
    public string Text
    {
        get => _textToDisplay;
        set
        {
            _textToDisplay = NormalizeNewlines(value);
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
            _shader = atlas.Shader;
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

    /// <inheritdoc/>
    public bool UseInlineTags
    {
        get => _useInlineTags;
        set
        {
            _useInlineTags = value;
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

        StyledChar[] chars = InlineTagParser.Parse(_textToDisplay, _useInlineTags);
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(chars, _fontStruct, Width, ScaleX, _extraSpaceWidth, _extraCharSpacing);
        float maxLineWidth = 0f;

        foreach (LineInfo line in lines)
        {
            float lw = TextLayout.MeasureLine(line.Chars, _fontStruct, ScaleX, _extraSpaceWidth, _extraCharSpacing);
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

    /// <summary>
    /// Canonicalises line endings to '\n' so layout, wrapping and the single-line newline
    /// check only ever have to deal with one form. Converts Windows ("\r\n") and classic-Mac
    /// ("\r") breaks; a lone '\r' would otherwise reach the font atlas as an undrawable glyph.
    /// </summary>
    private static string NormalizeNewlines(string text)
        => text.Replace("\r\n", "\n").Replace('\r', '\n');

    private IReadOnlyList<LineInfo> GetLines()
    {
        StyledChar[] chars = InlineTagParser.Parse(_textToDisplay, _useInlineTags);

        if (_multiline)
        {
            return TextLayout.ComputeLines(chars, _fontStruct, Width, ScaleX, _extraSpaceWidth, _extraCharSpacing);
        }

        // A newline can't be drawn as a glyph; in single-line mode it would otherwise crash deep in
        // the font atlas lookup with a message that wrongly suggests loading it. Fail here instead,
        // pointing at the real fix.
        foreach (StyledChar styledChar in chars)
        {
            if (styledChar.Char == '\n')
            {
                throw new InvalidOperationException(
                    "TextGraphic text contains a newline character (U+000A) but Multiline is false. " +
                    "Set Multiline = true to render text across multiple lines.");
            }
        }

        return [new LineInfo(chars, 0, chars.Length)];
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

            float lineWidth = TextLayout.MeasureLine(line.Chars, _fontStruct, ScaleX, _extraSpaceWidth, _extraCharSpacing);

            float charLeft = MathF.Round(HAlignment switch
            {
                HAlignment.Left or HAlignment.Full => 0,
                HAlignment.Centred => (Width - lineWidth) / 2,
                HAlignment.Right => Width - lineWidth,
                _ => throw new Exception($"TextGraphic/SetVerticesSimple: HAlignment {HAlignment} was not catered for."),
            });

            // Track the x-span of rendered characters on this line for whole-graphic decoration quads.
            float? decorationLeft = null;
            float decorationRight = 0f;

            // Track inline decoration spans (only used when the corresponding whole-graphic decoration is absent).
            float? inlineUlLeft = null;
            float inlineUlRight = 0f;
            OpenTK.Mathematics.Color4 inlineUlColour = default;
            float? inlineStLeft = null;
            float inlineStRight = 0f;
            OpenTK.Mathematics.Color4 inlineStColour = default;

            foreach (StyledChar sc in line.Chars)
            {
                char c = sc.Char;
                var atlasUV = _fontStruct.GetCharPositionNormalised(c);
                float glyphWidth = _fontStruct.GetCharPosition(c).Size.X * ScaleX;
                float charAdvance = glyphWidth + (c == ' ' ? _extraSpaceWidth : _extraCharSpacing);

                if (globalCharIndex >= _firstCharToDraw && globalCharIndex < visibleEnd)
                {
                    var charGlColour = sc.Style.ColourOverride.HasValue
                        ? sc.Style.ColourOverride.Value.ToOpenTK()
                        : glColour;

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
                            new Vertex(quadLeft,  quadTop,    charGlColour, uvLeft,  uvTop),
                            new Vertex(quadRight, quadTop,    charGlColour, uvRight, uvTop),
                            new Vertex(quadLeft,  quadBottom, charGlColour, uvLeft,  uvBottom),
                            new Vertex(quadRight, quadBottom, charGlColour, uvRight, uvBottom)
                        ));

                        // Extend inline underline span when no whole-graphic underline covers it.
                        if (!_underline.HasValue && sc.Style.Underline)
                        {
                            if (!inlineUlLeft.HasValue)
                            {
                                inlineUlLeft = quadLeft;
                                inlineUlColour = charGlColour;
                            }

                            inlineUlRight = MathF.Min(charLeft + charAdvance, Width);
                        }

                        // Extend inline strikethrough span when no whole-graphic strikethrough covers it.
                        if (!_strikethrough.HasValue && sc.Style.Strikethrough)
                        {
                            if (!inlineStLeft.HasValue)
                            {
                                inlineStLeft = quadLeft;
                                inlineStColour = charGlColour;
                            }

                            inlineStRight = MathF.Min(charLeft + charAdvance, Width);
                        }
                    }
                    else if (glyphWidth > 0) // zero-width glyphs (pure spacing) don't count as truncation
                    {
                        truncated = true;
                    }

                    // Close any inline decoration span when the style no longer requires it.
                    if (!_underline.HasValue && !sc.Style.Underline && inlineUlLeft.HasValue)
                    {
                        EmitInlineUnderline(inlineUlLeft.Value, inlineUlRight, lineTop, lineHeight, inlineUlColour, decorationVertices);
                        inlineUlLeft = null;
                    }

                    if (!_strikethrough.HasValue && !sc.Style.Strikethrough && inlineStLeft.HasValue)
                    {
                        EmitInlineStrikethrough(inlineStLeft.Value, inlineStRight, lineTop, lineHeight, inlineStColour, decorationVertices);
                        inlineStLeft = null;
                    }
                }
                else
                {
                    // Character is outside the visible range — close any open inline spans.
                    if (inlineUlLeft.HasValue)
                    {
                        EmitInlineUnderline(inlineUlLeft.Value, inlineUlRight, lineTop, lineHeight, inlineUlColour, decorationVertices);
                        inlineUlLeft = null;
                    }

                    if (inlineStLeft.HasValue)
                    {
                        EmitInlineStrikethrough(inlineStLeft.Value, inlineStRight, lineTop, lineHeight, inlineStColour, decorationVertices);
                        inlineStLeft = null;
                    }
                }

                charLeft += charAdvance;
                globalCharIndex++;
            }

            // Close any inline decoration spans that were still open at end of line.
            if (inlineUlLeft.HasValue)
            {
                EmitInlineUnderline(inlineUlLeft.Value, inlineUlRight, lineTop, lineHeight, inlineUlColour, decorationVertices);
            }

            if (inlineStLeft.HasValue)
            {
                EmitInlineStrikethrough(inlineStLeft.Value, inlineStRight, lineTop, lineHeight, inlineStColour, decorationVertices);
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

        Vertices = [..vertices];
        DecorationVertices = [..decorationVertices];

        if (truncated && !_wasTruncated)
        {
            string preview = _textToDisplay.Length > 40 ? string.Concat(_textToDisplay.AsSpan(0, 40), "...") : _textToDisplay;
            Logger.Warning($"TextGraphic text is truncated ({Width}x{Height}px): \"{preview}\"");
        }
        _wasTruncated = truncated;
    }

    private void EmitInlineStrikethrough(float left, float right, float lineTop, float lineHeight, OpenTK.Mathematics.Color4 colour, List<VertexNoTexture> decorationVertices)
    {
        const float thickness = 1f;
        float stTop    = lineTop + (lineHeight - thickness) / 2f;
        float stBottom = stTop + thickness;

        if (stTop < Height && stBottom > 0)
        {
            decorationVertices.Add(DecorationQuad(left, MathF.Max(stTop, 0f), right - left, MathF.Min(stBottom, Height) - MathF.Max(stTop, 0f), colour));
        }
    }

    private void EmitInlineUnderline(float left, float right, float lineTop, float lineHeight, OpenTK.Mathematics.Color4 colour, List<VertexNoTexture> decorationVertices)
    {
        const float thickness = 1f;
        float ulTop    = lineTop + lineHeight;
        float ulBottom = ulTop + thickness;

        if (ulTop < Height && ulBottom > 0)
        {
            decorationVertices.Add(DecorationQuad(left, MathF.Max(ulTop, 0f), right - left, MathF.Min(ulBottom, Height) - MathF.Max(ulTop, 0f), colour));
        }
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

        if (_angle != 0)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, _angle, Width / 2, Height / 2);
        }

        _shader.Bind();
        _vertexDataBuffer.Bind();
        _texture.Bind();
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);
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
