using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Renders an image texture masked by a stencil texture. Pixels where the stencil's alpha
/// channel is zero are discarded; elsewhere the image is sampled and multiplied by the stencil
/// alpha, producing soft edges when the stencil has intermediate alpha values.
/// </summary>
/// <param name="imageTexture">The texture to display. Not owned; not disposed with this graphic.</param>
/// <param name="stencilTexture">
/// The mask texture. The effective mask value is <c>alpha × red</c>, so two conventions are
/// supported: a PNG with transparency (alpha channel drives the mask, red is irrelevant where
/// alpha = 0), or a lossless black-and-white image (red channel drives the mask, alpha = 1
/// everywhere). Either way, white/opaque = show, black/transparent = discard.
/// Prefer lossless formats (PNG) — lossy formats (JPEG) introduce compression artefacts in
/// regions that should be fully masked. If you must use a lossy source, set
/// <see cref="Threshold"/> to a value such as 0.5 to discard the artefacts.
/// Not owned; not disposed with this graphic.
/// </param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
public sealed class StencilGraphic(ITexture imageTexture, ITexture stencilTexture, float x, float y, float width, float height, int layer = int.MaxValue)
    : GraphicBase(x, y, width, height, layer), IGraphic, IDisposable
{
    private readonly StencilRenderer _stencilRenderer = new(imageTexture, stencilTexture);
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;

    /// <inheritdoc/>
    public float Angle { get; set; } = 0f;

    /// <summary>
    /// Mask values at or below this threshold are fully discarded. Defaults to 0 (soft edges
    /// preserved). Set to 0.5 for hard-edged stencils from lossy sources such as JPEG, where
    /// mipmap blending introduces unwanted partial values in regions that should be fully masked.
    /// </summary>
    public float Threshold
    {
        get => _stencilRenderer.Threshold;
        set => _stencilRenderer.Threshold = value;
    }

    /// <summary>Tint colour multiplied with the image sample. Defaults to <see cref="Colour.White"/> (no tint).</summary>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
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
        if (_verticesChanged)
        {
            _stencilRenderer.UpdateVertices(Width, Height, _colour);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        if (Angle != 0f)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, Angle, Width / 2f, Height / 2f);
        }

        _stencilRenderer.Render(ref projection, ref mv);
    }

    #region Dispose
    private bool _disposedValue = false;

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _stencilRenderer.Dispose();
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
