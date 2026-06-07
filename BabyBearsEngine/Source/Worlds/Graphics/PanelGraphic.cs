using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A "9-slice" textured panel. The source texture is logically divided into nine regions by a
/// single border size: four fixed-size corners, four edges that stretch along one axis to span
/// the gap between corners, and a centre that stretches along both axes to fill the interior.
/// Drawing the panel at any size leaves the corners pixel-perfect and only stretches the edges
/// and centre — so border decorations stay sharp regardless of the panel's drawn size.
///
/// Example: a 150×150 source texture with <c>borderSize</c> 50 drawn at 200×200 renders each
/// corner at 50×50, the top/bottom edges stretched to 100×50, the left/right edges stretched
/// to 50×100, and the centre stretched to 100×100.
///
/// Construction allocates GL resources — must be created on the engine thread after the GL
/// context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
public sealed class PanelGraphic : GraphicBase, IGraphic, IDisposable
{
    private readonly ITexture _texture;
    private readonly float _borderSize;
    private readonly GraphicRenderer[] _sliceRenderers = new GraphicRenderer[9];
    private readonly PanelSlice[] _sliceLayout = new PanelSlice[9];
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <param name="texture">The texture to sample. Not owned by this graphic; not disposed when the graphic is disposed.</param>
    /// <param name="borderSize">Width and height in source-texture pixels of each corner slice. Must be > 0 and ≤ half the smaller texture dimension.</param>
    /// <param name="x">X position in the parent's local space.</param>
    /// <param name="y">Y position in the parent's local space.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public PanelGraphic(ITexture texture, float borderSize, float x, float y, float width, float height, int layer = int.MaxValue)
        : base(x, y, width, height, layer)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(borderSize);
        if (borderSize * 2f > texture.Width || borderSize * 2f > texture.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(borderSize), borderSize,
                $"borderSize must be ≤ half the texture's smaller dimension (texture is {texture.Width}×{texture.Height}).");
        }

        _texture = texture;
        _borderSize = borderSize;
        for (int sliceIndex = 0; sliceIndex < 9; sliceIndex++)
        {
            _sliceRenderers[sliceIndex] = new GraphicRenderer(texture);
        }
    }

    /// <param name="texture">The texture to sample. Not owned by this graphic; not disposed when the graphic is disposed.</param>
    /// <param name="borderSize">Width and height in source-texture pixels of each corner slice. Must be > 0 and ≤ half the smaller texture dimension.</param>
    /// <param name="rect">Position and size in the parent's local space.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public PanelGraphic(ITexture texture, float borderSize, Rect rect, int layer = int.MaxValue)
        : this(texture, borderSize, rect.X, rect.Y, rect.W, rect.H, layer)
    {
    }

    /// <summary>
    /// Convenience overload that picks <c>borderSize = texture.Width / 3</c>, i.e. treats the
    /// texture as three equal-thirds horizontally and vertically. Use the explicit-borderSize
    /// overload if the panel art has a non-thirds border layout.
    /// </summary>
    /// <param name="texture">The texture to sample. Not owned by this graphic; not disposed when the graphic is disposed.</param>
    /// <param name="x">X position in the parent's local space.</param>
    /// <param name="y">Y position in the parent's local space.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public PanelGraphic(ITexture texture, float x, float y, float width, float height, int layer = int.MaxValue)
        : this(texture, GetEqualThirdsBorderSize(texture), x, y, width, height, layer)
    {
    }

    /// <inheritdoc/>
    public float Angle { get; set; } = 0f;

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

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _verticesChanged = true;
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (_verticesChanged)
        {
            ComputeSliceLayout(_sliceLayout, Width, Height, _borderSize, _texture.Width, _texture.Height);
            for (int sliceIndex = 0; sliceIndex < 9; sliceIndex++)
            {
                PanelSlice slice = _sliceLayout[sliceIndex];
                _sliceRenderers[sliceIndex].UpdateVertices(slice.Width, slice.Height, _colour,
                    slice.U1, slice.V1, slice.U2, slice.V2);
            }
            _verticesChanged = false;
        }

        var baseModelView = Matrix3.Translate(ref modelView, X, Y);
        if (Angle != 0f)
        {
            baseModelView = Matrix3.RotateAroundPoint(ref baseModelView, Angle, Width / 2f, Height / 2f);
        }

        for (int sliceIndex = 0; sliceIndex < 9; sliceIndex++)
        {
            PanelSlice slice = _sliceLayout[sliceIndex];
            if (slice.Width <= 0f || slice.Height <= 0f)
            {
                continue;
            }

            var sliceModelView = Matrix3.Translate(ref baseModelView, slice.X, slice.Y);
            _sliceRenderers[sliceIndex].Render(ref projection, ref sliceModelView);
        }
    }

    /// <summary>
    /// Fills <paramref name="target"/> (length 9) with the per-slice destination rect (in
    /// panel-local pixels) and source UV rect (in [0,1]) for the 9-slice layout. Order is
    /// row-major top-to-bottom, left-to-right: [TL, TM, TR, ML, MM, MR, BL, BM, BR].
    /// </summary>
    internal static void ComputeSliceLayout(PanelSlice[] target, float panelWidth, float panelHeight,
        float borderSize, int textureWidth, int textureHeight)
    {
        float middleWidth = Math.Max(0f, panelWidth - 2f * borderSize);
        float middleHeight = Math.Max(0f, panelHeight - 2f * borderSize);

        float u0 = 0f;
        float u1 = borderSize / textureWidth;
        float u2 = (textureWidth - borderSize) / textureWidth;
        float u3 = 1f;
        float v0 = 0f;
        float v1 = borderSize / textureHeight;
        float v2 = (textureHeight - borderSize) / textureHeight;
        float v3 = 1f;

        float[] columnX = [0f, borderSize, panelWidth - borderSize];
        float[] columnW = [borderSize, middleWidth, borderSize];
        float[] columnU1 = [u0, u1, u2];
        float[] columnU2 = [u1, u2, u3];

        float[] rowY = [0f, borderSize, panelHeight - borderSize];
        float[] rowH = [borderSize, middleHeight, borderSize];
        float[] rowV1 = [v0, v1, v2];
        float[] rowV2 = [v1, v2, v3];

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int sliceIndex = row * 3 + col;
                target[sliceIndex] = new PanelSlice(
                    columnX[col], rowY[row], columnW[col], rowH[row],
                    columnU1[col], rowV1[row], columnU2[col], rowV2[row]);
            }
        }
    }

    private static float GetEqualThirdsBorderSize(ITexture texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        return texture.Width / 3f;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                for (int sliceIndex = 0; sliceIndex < 9; sliceIndex++)
                {
                    _sliceRenderers[sliceIndex].Dispose();
                }
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Destination rect (panel-local pixels) and source UV rect (in [0,1]) for one slice of a
/// <see cref="PanelGraphic"/>. Internal — exposed only to support unit tests of the layout
/// math without standing up GL state.
/// </summary>
internal readonly struct PanelSlice(float x, float y, float width, float height, float u1, float v1, float u2, float v2)
{
    public readonly float X = x;
    public readonly float Y = y;
    public readonly float Width = width;
    public readonly float Height = height;
    public readonly float U1 = u1;
    public readonly float V1 = v1;
    public readonly float U2 = u2;
    public readonly float V2 = v2;
}
