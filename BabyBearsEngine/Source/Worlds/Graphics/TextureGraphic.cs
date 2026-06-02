using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A textured rectangle drawn at a position and size, optionally tinted, alpha-blended, and rotated.
/// Construction allocates GL resources (vertex buffer, etc.) — must be created on the engine thread
/// after the GL context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
/// <param name="texture">The texture to sample. Not owned by this graphic; not disposed when the graphic is disposed.</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
public sealed class TextureGraphic(ITexture texture, float x, float y, float width, float height, int layer = int.MaxValue) : GraphicBase(x, y, width, height, layer), IGraphic, IDisposable
{
    private readonly GraphicRenderer _graphicRenderer = new(texture);
    private float _angle = 0;
    private Colour _colour = Colour.White;
    private Rect _sourceArea = Rect.UnitRect;
    private bool _verticesChanged = true;

    /// <param name="texture">The texture to sample. Not owned by this graphic; not disposed when the graphic is disposed.</param>
    /// <param name="rect">Position and size in the parent's local space.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public TextureGraphic(ITexture texture, Rect rect, int layer = int.MaxValue)
        : this(texture, rect.X, rect.Y, rect.W, rect.H, layer)
    {
    }

    /// <summary>Tint colour multiplied with the texture sample. Defaults to <see cref="Colour.White"/> (no tint).</summary>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Convenience accessor for the alpha component of <see cref="Colour"/>, expressed as a normalised float (0–1) on set; raw byte (0–255) on get.</summary>
    public float Alpha
    {
        get => Colour.A;
        set => Colour = new(Colour.R, Colour.G, Colour.B, (byte)Math.Round(value * 255f));
    }

    /// <summary>
    /// The shader program used to render this graphic. Defaults to a
    /// <see cref="StandardMatrixShaderProgram"/> (straight texture passthrough); assign a
    /// different <see cref="IMatrixShaderProgram"/> — e.g. <see cref="GreyscaleShaderProgram"/>,
    /// <see cref="DarkenShaderProgram"/>, <see cref="BlurShaderProgram"/> — to apply a
    /// fragment effect. Never null; assigning null throws <see cref="ArgumentNullException"/>.
    /// </summary>
    public IMatrixShaderProgram Shader
    {
        get => _graphicRenderer.Shader;
        set => _graphicRenderer.Shader = value;
    }

    /// <summary>Rotation angle in degrees, applied around the image's centre.</summary>
    public float Angle
    {
        get => _angle;
        set
        {
            _angle = value;
        }
    }

    /// <summary>
    /// The sub-region of the texture to display, expressed as fractions of the texture in
    /// [0, 1] on both axes. Defaults to the whole texture, <c>(0, 0, 1, 1)</c>. The region is
    /// drawn into the matching sub-region of the graphic's rectangle — e.g. a source area of
    /// <c>(0.5, 0.5, 0.5, 0.5)</c> draws the texture's bottom-right quarter into the
    /// bottom-right quarter of the graphic's rectangle. Must be a subset of the unit square.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The assigned area is not within the unit square (0, 0, 1, 1).</exception>
    public Rect SourceArea
    {
        get => _sourceArea;
        set
        {
            if (value.X < 0f || value.Y < 0f || value.W < 0f || value.H < 0f
                || value.Right > 1f || value.Bottom > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "SourceArea must be a subset of the unit square (0, 0, 1, 1).");
            }

            if (_sourceArea.Equals(value))
            {
                return;
            }

            _sourceArea = new Rect(value);
            _verticesChanged = true;
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _verticesChanged = true;
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        float drawWidth = Width * _sourceArea.W;
        float drawHeight = Height * _sourceArea.H;

        if (_verticesChanged)
        {
            _graphicRenderer.UpdateVertices(drawWidth, drawHeight, _colour,
                _sourceArea.Left, _sourceArea.Top, _sourceArea.Right, _sourceArea.Bottom);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X + Width * _sourceArea.X, Y + Height * _sourceArea.Y);

        if (_angle != 0)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, _angle, drawWidth / 2, drawHeight / 2);
        }

        _graphicRenderer.Render(ref projection, ref mv);
    }

    #region Dispose
    private bool _disposedValue;

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _graphicRenderer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
