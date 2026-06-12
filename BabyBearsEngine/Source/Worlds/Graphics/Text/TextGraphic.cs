using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.IO;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Buffers;
using BabyBearsEngine.Worlds.Cameras;
using BabyBearsEngine.Worlds.UI.Themes;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public sealed class TextGraphic : GraphicBase, IGraphic, ITextGraphic, IDisposable
{
    // How close to 1:1 the scale must be before glyph quads are snapped to the pixel grid. A small
    // tolerance absorbs floating-point drift (e.g. a camera tile size that divides to 0.9999) so
    // text drawn at native size still snaps, while genuinely scaled text does not.
    private const float ScaleSnapTolerance = 0.001f;

    // Convention used by BoldFont/ItalicFont/BoldItalicFont auto-discovery: a companion file named
    // "{baseFontName}_b.ttf", "{baseFontName}_i.ttf", or "{baseFontName}_bi.ttf" in Assets/Fonts/ is
    // picked up automatically when the base Font is assigned.
    private const string DefaultFontFolder = "Assets/Fonts/";
    private const string BoldSuffix = "_b";
    private const string ItalicSuffix = "_i";
    private const string BoldItalicSuffix = "_bi";

    private readonly SolidColourShaderProgramMatrix _decorationShader = Shaders.SolidColour;
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly VertexDataBuffer<Vertex> _boldVertexDataBuffer = new();
    private readonly VertexDataBuffer<Vertex> _italicVertexDataBuffer = new();
    private readonly VertexDataBuffer<Vertex> _boldItalicVertexDataBuffer = new();
    private readonly VertexDataBuffer<VertexNoTexture> _decorationVertexDataBuffer = new();
    private IMatrixShaderProgram _shader;
    private ITexture _texture;
    private FontAtlasMetrics _metrics;
    private FontDefinition? _boldFontDef;
    private FontAtlasMetrics? _boldMetrics;
    private ITexture? _boldTexture;
    private IMatrixShaderProgram? _boldShader;
    private FontDefinition? _italicFontDef;
    private FontAtlasMetrics? _italicMetrics;
    private ITexture? _italicTexture;
    private IMatrixShaderProgram? _italicShader;
    private FontDefinition? _boldItalicFontDef;
    private FontAtlasMetrics? _boldItalicMetrics;
    private ITexture? _boldItalicTexture;
    private IMatrixShaderProgram? _boldItalicShader;
    private bool _warnedMissingBold = false;
    private bool _warnedMissingItalic = false;
    private bool _warnedMissingBoldItalic = false;
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

        FontAtlas atlas = FontTextureCache.GetOrCreate(fontDef);
        _fontDef = fontDef;
        _metrics = atlas.Metrics;
        _texture = atlas.Texture;
        _shader = atlas.Shader;

        SetBoldFontInternal(TryAutoDiscoverVariant(fontDef, BoldSuffix));
        SetItalicFontInternal(TryAutoDiscoverVariant(fontDef, ItalicSuffix));
        SetBoldItalicFontInternal(TryAutoDiscoverVariant(fontDef, BoldItalicSuffix));
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
            FontAtlas atlas = FontTextureCache.GetOrCreate(value);
            _fontDef = value;
            _metrics = atlas.Metrics;
            _texture = atlas.Texture;
            _shader = atlas.Shader;
            _warnedMissingBold = false;
            _warnedMissingItalic = false;
            _warnedMissingBoldItalic = false;

            // Re-run auto-discovery for the new base font. Variants that the caller previously set
            // explicitly are preserved — we only refresh slots that are currently null.
            if (_boldFontDef is null)
            {
                SetBoldFontInternal(TryAutoDiscoverVariant(value, BoldSuffix));
            }

            if (_italicFontDef is null)
            {
                SetItalicFontInternal(TryAutoDiscoverVariant(value, ItalicSuffix));
            }

            if (_boldItalicFontDef is null)
            {
                SetBoldItalicFontInternal(TryAutoDiscoverVariant(value, BoldItalicSuffix));
            }

            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public FontDefinition? BoldFont
    {
        get => _boldFontDef;
        set
        {
            SetBoldFontInternal(value);
            _warnedMissingBold = false;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public FontDefinition? ItalicFont
    {
        get => _italicFontDef;
        set
        {
            SetItalicFontInternal(value);
            _warnedMissingItalic = false;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public FontDefinition? BoldItalicFont
    {
        get => _boldItalicFontDef;
        set
        {
            SetBoldItalicFontInternal(value);
            _warnedMissingBoldItalic = false;
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
    private Vertex[] BoldVertices { get; set; } = [];
    private Vertex[] ItalicVertices { get; set; } = [];
    private Vertex[] BoldItalicVertices { get; set; } = [];

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
    public Point MeasureString(string text) => MeasureString(text.AsSpan());

    /// <summary>
    /// Returns the rendered size of <paramref name="text"/> in pixels at the current scale.
    /// Allocation-free overload — callers measuring a slice of a larger string can pass an
    /// <c>AsSpan</c> view instead of allocating a <c>Substring</c>.
    /// </summary>
    public Point MeasureString(ReadOnlySpan<char> text)
    {
        var raw = _metrics.MeasureString(text);
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
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(chars, _metrics, Width, ScaleX, _extraSpaceWidth, _extraCharSpacing);
        float maxLineWidth = 0f;

        foreach (LineInfo line in lines)
        {
            float lw = TextLayout.MeasureLine(line.Chars, _metrics, ScaleX, _extraSpaceWidth, _extraCharSpacing);
            if (lw > maxLineWidth)
            {
                maxLineWidth = lw;
            }
        }

        return new Point(maxLineWidth, lines.Count * (_metrics.HighestChar * ScaleY + _extraLineSpacing));
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

    /// <summary>
    /// Whether glyph quads should be snapped to whole-pixel positions. Only when the text is drawn
    /// 1:1 and unrotated: at native scale, snapping keeps hinted, point-sampled glyphs landing exactly
    /// on the pixel grid (so nearest sampling never drops or duplicates an edge column when fractional
    /// spacing nudges a glyph off-grid). Snapping a scaled or rotated quad to the integer grid would
    /// instead distort its shape or spacing, so it is suppressed there.
    /// </summary>
    internal static bool ShouldPixelSnap(float scaleX, float scaleY, float angle)
        => angle == 0f
           && MathF.Abs(scaleX - 1f) < ScaleSnapTolerance
           && MathF.Abs(scaleY - 1f) < ScaleSnapTolerance;

    /// <summary>
    /// Whether a glyph's logical content falls outside the graphic's bounds and is therefore cut
    /// off. Truncation is judged on the glyph's advance cell — horizontally
    /// [<paramref name="charLeft"/>, charLeft + <paramref name="charAdvance"/>] and vertically the
    /// line box [<paramref name="lineTop"/>, lineTop + <paramref name="lineHeight"/>] — not on the
    /// render quad. The SDF backend inflates the render quad with transparent "glow" margins (via
    /// negative bearings) that legitimately spill past the bounds without any text being lost, so
    /// keying off the quad would flag those false positives. The GDI backend has zero bearings and a
    /// quad equal to the advance cell, so its behaviour is unchanged.
    /// </summary>
    internal static bool IsContentTruncated(
        float charLeft, float charAdvance, float lineTop, float lineHeight, float width, float height)
        => charLeft < 0f
           || charLeft + charAdvance > width
           || lineTop < 0f
           || lineTop + lineHeight > height;

    private IReadOnlyList<LineInfo> GetLines()
    {
        StyledChar[] chars = InlineTagParser.Parse(_textToDisplay, _useInlineTags);

        if (_multiline)
        {
            return TextLayout.ComputeLines(chars, _metrics, Width, ScaleX, _extraSpaceWidth, _extraCharSpacing);
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

    private void SetBoldFontInternal(FontDefinition? value)
    {
        _boldFontDef = value;

        if (value is not null)
        {
            FontAtlas atlas = FontTextureCache.GetOrCreate(value);
            _boldMetrics = atlas.Metrics;
            _boldTexture = atlas.Texture;
            _boldShader = atlas.Shader;
        }
        else
        {
            _boldMetrics = null;
            _boldTexture = null;
            _boldShader = null;
        }
    }

    private void SetItalicFontInternal(FontDefinition? value)
    {
        _italicFontDef = value;

        if (value is not null)
        {
            FontAtlas atlas = FontTextureCache.GetOrCreate(value);
            _italicMetrics = atlas.Metrics;
            _italicTexture = atlas.Texture;
            _italicShader = atlas.Shader;
        }
        else
        {
            _italicMetrics = null;
            _italicTexture = null;
            _italicShader = null;
        }
    }

    private void SetBoldItalicFontInternal(FontDefinition? value)
    {
        _boldItalicFontDef = value;

        if (value is not null)
        {
            FontAtlas atlas = FontTextureCache.GetOrCreate(value);
            _boldItalicMetrics = atlas.Metrics;
            _boldItalicTexture = atlas.Texture;
            _boldItalicShader = atlas.Shader;
        }
        else
        {
            _boldItalicMetrics = null;
            _boldItalicTexture = null;
            _boldItalicShader = null;
        }
    }

    /// <summary>
    /// Looks for a companion font file alongside <paramref name="baseFont"/> following the
    /// <c>{FontName}_b.ttf</c> / <c>{FontName}_i.ttf</c> convention. Returns a clone of the base
    /// FontDefinition pointing at the new file when found, or <see langword="null"/> otherwise.
    /// </summary>
    internal static FontDefinition? TryAutoDiscoverVariant(FontDefinition baseFont, string suffix)
    {
        if (string.IsNullOrEmpty(baseFont.FontName))
        {
            return null;
        }

        string candidateName = baseFont.FontName + suffix;
        string candidatePath = DefaultFontFolder + candidateName + ".ttf";

        if (!Files.FileExists(candidatePath))
        {
            return null;
        }

        // The file IS the bold/italic design, so keep FontStyle.Regular — passing Bold/Italic here
        // would ask the renderer to synthesise the style on top of an already-styled face.
        return baseFont with { FontName = candidateName };
    }

    /// <summary>
    /// Chooses which atlas (metrics and vertex list) to use for a styled character. When both
    /// <c>&lt;b&gt;</c> and <c>&lt;i&gt;</c> are active, prefers <see cref="BoldItalicFont"/>; if that is not
    /// registered, falls through to <see cref="BoldFont"/>, then <see cref="ItalicFont"/>, then the
    /// base font, logging a one-shot warning whenever the slot for the requested style is missing.
    /// </summary>
    private (FontAtlasMetrics Metrics, List<Vertex> Output) SelectAtlasForStyle(
        InlineTagStyle style,
        List<Vertex> regular,
        List<Vertex> bold,
        List<Vertex> italic,
        List<Vertex> boldItalic)
    {
        if (style.Bold && style.Italic)
        {
            if (_boldItalicMetrics is not null)
            {
                return (_boldItalicMetrics, boldItalic);
            }

            if (!_warnedMissingBoldItalic)
            {
                Logger.Warning($"TextGraphic: <b><i> span encountered but BoldItalicFont is not set on font '{_fontDef.FontName}' — falling back to bold/italic/base.");
                _warnedMissingBoldItalic = true;
            }
        }

        if (style.Bold)
        {
            if (_boldMetrics is not null)
            {
                return (_boldMetrics, bold);
            }

            if (!_warnedMissingBold)
            {
                Logger.Warning($"TextGraphic: <b> span encountered but BoldFont is not set on font '{_fontDef.FontName}' — falling back to base font.");
                _warnedMissingBold = true;
            }
        }

        if (style.Italic)
        {
            if (_italicMetrics is not null)
            {
                return (_italicMetrics, italic);
            }

            if (!_warnedMissingItalic)
            {
                Logger.Warning($"TextGraphic: <i> span encountered but ItalicFont is not set on font '{_fontDef.FontName}' — falling back to base font.");
                _warnedMissingItalic = true;
            }
        }

        return (_metrics, regular);
    }

    private void SetVertices()
    {
        List<Vertex> vertices = [];
        List<Vertex> boldVertices = [];
        List<Vertex> italicVertices = [];
        List<Vertex> boldItalicVertices = [];
        List<VertexNoTexture> decorationVertices = [];

        var glColour = Colour.ToOpenTK();
        bool snapToPixelGrid = ShouldPixelSnap(_scaleX, _scaleY, _angle);
        float lineHeight = ScaleY * _metrics.HighestChar;
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

            float lineWidth = TextLayout.MeasureLine(line.Chars, _metrics, ScaleX, _extraSpaceWidth, _extraCharSpacing);

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
                (FontAtlasMetrics charMetrics, List<Vertex> charVertexOutput) = SelectAtlasForStyle(sc.Style, vertices, boldVertices, italicVertices, boldItalicVertices);
                var atlasUV = charMetrics.GetCharPositionNormalised(c);
                var renderBox = charMetrics.GetCharPosition(c);
                var bearing = charMetrics.GetCharBearing(c);
                float renderWidth = renderBox.Size.X * ScaleX;
                float renderHeight = renderBox.Size.Y * ScaleY;
                float charAdvance = charMetrics.GetCharAdvance(c) * ScaleX + (c == ' ' ? _extraSpaceWidth : _extraCharSpacing);

                if (globalCharIndex >= _firstCharToDraw && globalCharIndex < visibleEnd)
                {
                    var charGlColour = sc.Style.ColourOverride.HasValue
                        ? sc.Style.ColourOverride.Value.ToOpenTK()
                        : glColour;

                    // Flag truncation on the glyph's logical advance cell, not its render quad: the
                    // SDF quad's transparent glow margin spills past the bounds by design and must
                    // not count as lost text. Spaces (no ink) never count.
                    if (renderWidth > 0 && IsContentTruncated(charLeft, charAdvance, lineTop, lineHeight, Width, Height))
                    {
                        truncated = true;
                    }

                    // The render quad spans the full glyph bitmap including the SDF "glow" margins,
                    // placed at the pen plus the glyph's bearing. The glow extends outside the advance
                    // box (and may overlap neighbouring glyphs) rather than being clipped to the cell —
                    // overlap is harmless because the shader resolves the glow to zero alpha beyond ~1px
                    // of the edge. The pen still advances by the font advance, kept separate below.
                    float quadLeft   = charLeft + bearing.X * ScaleX;
                    float quadTop    = lineTop + bearing.Y * ScaleY;

                    // At native, unrotated scale, snap the quad's top-left to whole pixels so the
                    // glyph lands exactly on the grid. Width/height are already whole pixels here, so
                    // the right/bottom edges follow without rounding (which would risk a ±1px width).
                    if (snapToPixelGrid)
                    {
                        quadLeft = MathF.Round(quadLeft);
                        quadTop = MathF.Round(quadTop);
                    }

                    float quadRight  = quadLeft + renderWidth;
                    float quadBottom = quadTop + renderHeight;

                    if (renderWidth > 0 && quadRight > 0 && quadLeft < Width && quadBottom > 0 && quadTop < Height)
                    {
                        float uvLeft   = atlasUV.Min.X;
                        float uvTop    = atlasUV.Min.Y;
                        float uvRight  = atlasUV.Max.X;
                        float uvBottom = atlasUV.Max.Y;

                        // Clip the quad to graphic bounds, adjusting UVs proportionally to sample only
                        // the visible slice. This is geometry only — whether the clip counts as lost
                        // text is decided above on the logical advance cell, so the SDF glow margin
                        // (which is allowed to spill past the edge) is clipped here without warning.
                        if (quadLeft < 0)
                        {
                            uvLeft += (uvRight - uvLeft) * -quadLeft / (quadRight - quadLeft);
                            quadLeft = 0;
                        }
                        if (quadRight > Width)
                        {
                            uvRight -= (uvRight - uvLeft) * (quadRight - Width) / (quadRight - quadLeft);
                            quadRight = Width;
                        }
                        if (quadTop < 0)
                        {
                            uvTop += (uvBottom - uvTop) * -quadTop / (quadBottom - quadTop);
                            quadTop = 0;
                        }
                        if (quadBottom > Height)
                        {
                            uvBottom -= (uvBottom - uvTop) * (quadBottom - Height) / (quadBottom - quadTop);
                            quadBottom = Height;
                        }

                        // Decorations key to the pen position, not the glow-shifted quad: the SDF
                        // glow margin extends the quad beyond the glyph's logical column and must not
                        // drag the underline/strikethrough start with it.
                        decorationLeft ??= MathF.Max(charLeft, 0f);
                        // Advance covers spacing after the glyph; decorations span the full advance, not just glyph width.
                        decorationRight = MathF.Min(charLeft + charAdvance, Width);

                        charVertexOutput.Add(GeometryHelper.QuadToTris(
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
                                inlineUlLeft = MathF.Max(charLeft, 0f);
                                inlineUlColour = charGlColour;
                            }

                            inlineUlRight = MathF.Min(charLeft + charAdvance, Width);
                        }

                        // Extend inline strikethrough span when no whole-graphic strikethrough covers it.
                        if (!_strikethrough.HasValue && sc.Style.Strikethrough)
                        {
                            if (!inlineStLeft.HasValue)
                            {
                                inlineStLeft = MathF.Max(charLeft, 0f);
                                inlineStColour = charGlColour;
                            }

                            inlineStRight = MathF.Min(charLeft + charAdvance, Width);
                        }
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
        BoldVertices = [..boldVertices];
        ItalicVertices = [..italicVertices];
        BoldItalicVertices = [..boldItalicVertices];
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
            _boldVertexDataBuffer.SetNewVertices(BoldVertices);
            _italicVertexDataBuffer.SetNewVertices(ItalicVertices);
            _boldItalicVertexDataBuffer.SetNewVertices(BoldItalicVertices);
            _decorationVertexDataBuffer.SetNewVertices(DecorationVertices);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        if (_angle != 0)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, _angle, Width / 2, Height / 2);
        }

        if (Vertices.Length > 0)
        {
            _shader.Bind();
            _vertexDataBuffer.Bind();
            _texture.Bind();
            _shader.SetProjectionMatrix(ref projection);
            _shader.SetModelViewMatrix(ref mv);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        }

        if (BoldVertices.Length > 0 && _boldShader is not null && _boldTexture is not null)
        {
            _boldShader.Bind();
            _boldVertexDataBuffer.Bind();
            _boldTexture.Bind();
            _boldShader.SetProjectionMatrix(ref projection);
            _boldShader.SetModelViewMatrix(ref mv);
            GL.DrawArrays(PrimitiveType.Triangles, 0, BoldVertices.Length);
        }

        if (ItalicVertices.Length > 0 && _italicShader is not null && _italicTexture is not null)
        {
            _italicShader.Bind();
            _italicVertexDataBuffer.Bind();
            _italicTexture.Bind();
            _italicShader.SetProjectionMatrix(ref projection);
            _italicShader.SetModelViewMatrix(ref mv);
            GL.DrawArrays(PrimitiveType.Triangles, 0, ItalicVertices.Length);
        }

        if (BoldItalicVertices.Length > 0 && _boldItalicShader is not null && _boldItalicTexture is not null)
        {
            _boldItalicShader.Bind();
            _boldItalicVertexDataBuffer.Bind();
            _boldItalicTexture.Bind();
            _boldItalicShader.SetProjectionMatrix(ref projection);
            _boldItalicShader.SetModelViewMatrix(ref mv);
            GL.DrawArrays(PrimitiveType.Triangles, 0, BoldItalicVertices.Length);
        }

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
                _boldVertexDataBuffer.Dispose();
                _italicVertexDataBuffer.Dispose();
                _boldItalicVertexDataBuffer.Dispose();
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
