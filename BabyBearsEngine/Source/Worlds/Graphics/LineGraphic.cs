using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A straight, coloured line segment between two points, extruded to a given thickness by the
/// geometry shader. Optionally dashed via <see cref="DashLength"/>/<see cref="GapLength"/> — a
/// distance-along-the-line value is computed by the geometry shader and fed to a dash-pattern
/// test in the fragment shader; <see cref="GapLength"/> 0 (the default) is a plain solid line.
/// <see cref="DashOffset"/> shifts the pattern along the line — animate it for a scrolling
/// "marching ants" effect, or use <see cref="MovingDashedLineGraphic"/> to have that done
/// automatically. Not sealed, so subclasses like that one can add ticking behaviour; construction
/// allocates GL resources (vertex buffer) — must be created on the engine thread after the GL
/// context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
/// <param name="start">Line start point, in the parent's local space.</param>
/// <param name="end">Line end point, in the parent's local space.</param>
/// <param name="colour">Line colour.</param>
/// <param name="thickness">Full line width — in pixels if <paramref name="thicknessInPixels"/>, otherwise in local-space units.</param>
/// <param name="thicknessInPixels">True: <paramref name="thickness"/> is a constant screen-space pixel width, unaffected by scaling. False: thickness scales with the model-view transform.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
public class LineGraphic(Point start, Point end, Colour colour, float thickness, bool thicknessInPixels = true, int layer = int.MaxValue)
    : GraphicBase(start.X, start.Y, end.X - start.X, end.Y - start.Y, layer), IGraphic, IColourGraphic, IDisposable
{
    private readonly LineShaderProgram _shader = Shaders.Line;
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <inheritdoc/>
    public float Angle { get; set; } = 0f;

    /// <summary>Line colour.</summary>
    public Colour Colour
    {
        get => colour;
        set
        {
            colour = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Dash length along the line. Irrelevant when <see cref="GapLength"/> is 0 (the default — a plain solid line).</summary>
    public float DashLength { get; set; } = 1f;

    /// <summary>Shifts the dash pattern along the line. Irrelevant when <see cref="GapLength"/> is 0.</summary>
    public float DashOffset { get; set; } = 0f;

    /// <summary>Line end point, in the parent's local space. Moving this keeps <see cref="Start"/> fixed.</summary>
    public Point End
    {
        get => new(X + Width, Y + Height);
        set
        {
            Width = value.X - X;
            Height = value.Y - Y;
        }
    }

    /// <summary>Gap length between dashes. 0 (the default) draws a plain solid line.</summary>
    public float GapLength { get; set; } = 0f;

    /// <summary>Line start point, in the parent's local space. Moving this keeps <see cref="End"/> fixed.</summary>
    public Point Start
    {
        get => new(X, Y);
        set
        {
            Point endPoint = End;
            X = value.X;
            Y = value.Y;
            Width = endPoint.X - X;
            Height = endPoint.Y - Y;
        }
    }

    /// <summary>Full line width — in pixels if <see cref="ThicknessInPixels"/>, otherwise in local-space units.</summary>
    public float Thickness { get; set; } = thickness;

    /// <summary>True: <see cref="Thickness"/> is a constant screen-space pixel width, unaffected by scaling. False: thickness scales with the model-view transform.</summary>
    public bool ThicknessInPixels { get; set; } = thicknessInPixels;

    private Vertex[] Vertices
    {
        get
        {
            var colourTK = colour.ToOpenTK();

            return
            [
                new(0, 0, colourTK, 0, 0),
                new(Width, Height, colourTK, 0, 0),
            ];
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

        if (_verticesChanged)
        {
            _vertexDataBuffer.SetNewVertices(Vertices);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        if (Angle != 0f)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, Angle, Width / 2f, Height / 2f);
        }

        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);
        _shader.SetLineThickness(Thickness);
        _shader.SetThicknessInPixels(ThicknessInPixels);
        _shader.SetDashLength(DashLength);
        _shader.SetGapLength(GapLength);
        _shader.SetDashOffset(DashOffset);

        GL.DrawArrays(PrimitiveType.Lines, 0, 2);
    }

    protected virtual void Dispose(bool disposing)
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
