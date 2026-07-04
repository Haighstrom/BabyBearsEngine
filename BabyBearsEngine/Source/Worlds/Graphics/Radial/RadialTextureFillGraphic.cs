using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A textured radial fill mesh (pie sector or ring/annulus) used as the fill graphic of a
/// <see cref="RadialProgressBar"/>. The texture is polar-mapped rather than sampled from the
/// flat XY plane: U follows the angle around the full circle and V follows the radius from
/// the inner edge (the centre for <see cref="RadialFillStyle.Pie"/>) to the outer edge — see
/// <see cref="RadialMeshVertex"/>. This keeps the texture undistorted as the sweep grows,
/// instead of a planar sample stretching or smearing across the sector. Construction allocates
/// GL resources (vertex buffer, shader binding) — must be created on the engine thread after
/// the GL context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
public sealed class RadialTextureFillGraphic : GraphicBase, IGraphic, IDisposable
{
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly RadialFillStyle _fillStyle;
    private readonly float _ringThickness;
    private readonly float _startAngleDegrees;
    private readonly RadialSweepDirection _direction;
    private readonly int _segments;
    private ITexture _texture;
    private IMatrixShaderProgram _shader = Shaders.Standard;
    private Colour _colour = Colour.White;
    private float _amountFilled = 0f;
    private int _vertexCount = 0;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <param name="texture">The texture to sample, polar-mapped across the sector. Not owned by this graphic; not disposed when the graphic is disposed.</param>
    /// <param name="rect">Position and size in the parent's local space.</param>
    /// <param name="fillStyle">Pie or ring.</param>
    /// <param name="ringThickness">Ring band thickness as a fraction of the outer radius, in (0, 1]. Ignored for <see cref="RadialFillStyle.Pie"/>.</param>
    /// <param name="startAngleDegrees">Sweep start angle (clock convention — see <see cref="RadialFillMeshBuilder"/>).</param>
    /// <param name="direction">Sweep direction from the start angle.</param>
    /// <param name="segments">Arc segments for a full 0..1 sweep. Must be ≥ 1.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public RadialTextureFillGraphic(ITexture texture, Rect rect, RadialFillStyle fillStyle, float ringThickness, float startAngleDegrees, RadialSweepDirection direction, int segments, int layer = int.MaxValue)
        : base(rect.X, rect.Y, rect.W, rect.H, layer)
    {
        _texture = texture;
        _fillStyle = fillStyle;
        _ringThickness = ringThickness;
        _startAngleDegrees = startAngleDegrees;
        _direction = direction;
        _segments = segments;
    }

    /// <inheritdoc/>
    public float Angle { get; set; } = 0f;

    /// <summary>
    /// The texture sampled when rendering. Never null; assigning null throws
    /// <see cref="ArgumentNullException"/>. Not owned by this graphic.
    /// </summary>
    public ITexture Texture
    {
        get => _texture;
        set => _texture = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The shader program used to render this graphic. Defaults to a
    /// <see cref="StandardMatrixShaderProgram"/> (straight texture passthrough). Never null;
    /// assigning null throws <see cref="ArgumentNullException"/>.
    /// </summary>
    public IMatrixShaderProgram Shader
    {
        get => _shader;
        set => _shader = value ?? throw new ArgumentNullException(nameof(value));
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

    /// <summary>Sweep amount in [0, 1]. Not clamped here — <see cref="RadialProgressBar"/> owns clamping.</summary>
    public float AmountFilled
    {
        get => _amountFilled;
        set
        {
            if (_amountFilled == value)
            {
                return;
            }

            _amountFilled = value;
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
        _shader.Bind();
        _vertexDataBuffer.Bind();
        _texture.Bind();

        if (_verticesChanged)
        {
            RadialMeshVertex[] meshVertices = RadialFillMeshBuilder.Build(Width, Height, _amountFilled, _fillStyle, _ringThickness, _startAngleDegrees, _direction, _segments);
            _vertexCount = meshVertices.Length;

            var colourTK = _colour.ToOpenTK();
            Vertex[] vertices = new Vertex[meshVertices.Length];
            for (int vertexIndex = 0; vertexIndex < meshVertices.Length; vertexIndex++)
            {
                RadialMeshVertex meshVertex = meshVertices[vertexIndex];
                vertices[vertexIndex] = new Vertex(meshVertex.X, meshVertex.Y, colourTK, meshVertex.U, meshVertex.V);
            }

            _vertexDataBuffer.SetNewVertices(vertices);
            _verticesChanged = false;
        }

        if (_vertexCount == 0)
        {
            return;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        if (Angle != 0f)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, Angle, Width / 2f, Height / 2f);
        }

        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);

        PrimitiveType primitiveType = _fillStyle == RadialFillStyle.Ring ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleFan;
        GL.DrawArrays(primitiveType, 0, _vertexCount);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _vertexDataBuffer.Dispose();
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
