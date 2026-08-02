using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A connected sequence of straight, coloured line segments through a list of points, mitered at
/// each interior vertex. Pass the same point as both the first and last entry to close the path
/// into a loop. Optionally dashed via <see cref="DashLength"/>/<see cref="GapLength"/> — each
/// vertex carries its cumulative distance from the path's start, fed to a dash-pattern test in
/// the fragment shader; <see cref="GapLength"/> 0 (the default) is a plain solid path.
/// <see cref="DashOffset"/> shifts the pattern along the path — animate it for a scrolling
/// "marching ants" effect, or use <see cref="MovingDashedLinePathGraphic"/> to have that done
/// automatically. Not sealed, so subclasses like that one can add ticking behaviour; construction
/// allocates GL resources (vertex buffer) — must be created on the engine thread after the GL
/// context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
public class LinePathGraphic : GraphicBase, IGraphic, IColourGraphic, IDisposable
{
    private readonly LinePathShaderProgram _shader = Shaders.LinePath;
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly List<Point> _points;
    private Colour _colour;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <param name="points">
    /// The path's vertices, in the parent's local space. Consecutive points are joined by a
    /// straight segment, mitered at the shared vertex. Must contain at least 2 points. Pass the
    /// same point as both the first and last entry to close the path into a loop.
    /// </param>
    /// <param name="colour">Line colour.</param>
    /// <param name="thickness">Full line width — in pixels if <paramref name="thicknessInPixels"/>, otherwise in local-space units.</param>
    /// <param name="thicknessInPixels">True: <paramref name="thickness"/> is a constant screen-space pixel width, unaffected by scaling. False: thickness scales with the model-view transform.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public LinePathGraphic(IReadOnlyList<Point> points, Colour colour, float thickness, bool thicknessInPixels = true, int layer = int.MaxValue)
        : base(layer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(points.Count, 2, nameof(points));

        _points = [.. points];
        _colour = colour;
        Thickness = thickness;
        ThicknessInPixels = thicknessInPixels;

        UpdateBounds();
    }

    /// <inheritdoc/>
    public float Angle { get; set; } = 0f;

    /// <summary>Line colour.</summary>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Dash length along the path. Irrelevant when <see cref="GapLength"/> is 0 (the default — a plain solid path).</summary>
    public float DashLength { get; set; } = 1f;

    /// <summary>Shifts the dash pattern along the path. Irrelevant when <see cref="GapLength"/> is 0.</summary>
    public float DashOffset { get; set; } = 0f;

    /// <summary>Gap length between dashes. 0 (the default) draws a plain solid path.</summary>
    public float GapLength { get; set; } = 0f;

    /// <summary>The path's current vertices, in the parent's local space.</summary>
    public IReadOnlyList<Point> Points => _points;

    /// <summary>Full line width — in pixels if <see cref="ThicknessInPixels"/>, otherwise in local-space units.</summary>
    public float Thickness { get; set; }

    /// <summary>True: <see cref="Thickness"/> is a constant screen-space pixel width, unaffected by scaling. False: thickness scales with the model-view transform.</summary>
    public bool ThicknessInPixels { get; set; }

    /// <summary>
    /// Appends a single point to the end of the path without touching the rest — cheaper than
    /// <see cref="SetPoints"/> for a path built up incrementally (e.g. freehand drawing), since it
    /// doesn't copy the existing points. The GL vertex buffer is still rebuilt and re-uploaded in
    /// full on the next <see cref="Render"/>; see
    /// <see href="https://github.com/Haighstrom/BabyBearsEngine/issues/295">#295</see> for making
    /// that incremental too.
    /// </summary>
    public void AppendPoint(Point point)
    {
        _points.Add(point);
        ExtendBounds(point);
        _verticesChanged = true;
    }

    private Vertex[] BuildVertices()
    {
        int pointCount = _points.Count;
        var colourTK = _colour.ToOpenTK();

        // GL_LINE_STRIP_ADJACENCY draws a segment per interior (vertex[i], vertex[i+1]) window
        // using its neighbours to compute the miter, so the real points need one extra vertex of
        // adjacency context at each end.
        Vertex[] vertices = new Vertex[pointCount + 2];

        // The geometry shader only ever sees a local 4-point window, so it can't know how far
        // along the whole path a segment sits — that has to be computed here and carried through
        // as a per-vertex attribute (piggybacking on the unused U texcoord slot) for the dash
        // pattern in dashed_line.frag.
        float cumulativeDistance = 0f;
        for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
        {
            if (pointIndex > 0)
            {
                cumulativeDistance += (_points[pointIndex] - _points[pointIndex - 1]).Length;
            }

            vertices[pointIndex + 1] = new Vertex(_points[pointIndex].X - X, _points[pointIndex].Y - Y, colourTK, cumulativeDistance, 0);
        }

        if (_points[0] == _points[pointCount - 1])
        {
            // Closed loop: wrap the adjacency context around to the path's real neighbours so the
            // seam miters the same as any other interior vertex.
            Point before = _points[pointCount - 2];
            Point after = _points[1];
            vertices[0] = new Vertex(before.X - X, before.Y - Y, colourTK, 0, 0);
            vertices[pointCount + 1] = new Vertex(after.X - X, after.Y - Y, colourTK, 0, 0);
        }
        else
        {
            // Open path: extrapolate straight past each end point so the tip isn't mitered.
            Point before = 2 * _points[0] - _points[1];
            Point after = 2 * _points[pointCount - 1] - _points[pointCount - 2];
            vertices[0] = new Vertex(before.X - X, before.Y - Y, colourTK, 0, 0);
            vertices[pointCount + 1] = new Vertex(after.X - X, after.Y - Y, colourTK, 0, 0);
        }

        return vertices;
    }

    private void ExtendBounds(Point point)
    {
        float minX = Math.Min(X, point.X);
        float maxX = Math.Max(X + Width, point.X);
        float minY = Math.Min(Y, point.Y);
        float maxY = Math.Max(Y + Height, point.Y);

        X = minX;
        Y = minY;
        Width = maxX - minX;
        Height = maxY - minY;
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
            _vertexDataBuffer.SetNewVertices(BuildVertices());
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        if (Angle != 0f)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, Angle, Width / 2f, Height / 2f);
        }

        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);
        _shader.SetThickness(Thickness);
        _shader.SetThicknessInPixels(ThicknessInPixels);
        _shader.SetDashLength(DashLength);
        _shader.SetGapLength(GapLength);
        _shader.SetDashOffset(DashOffset);

        GL.DrawArrays(PrimitiveType.LineStripAdjacency, 0, _points.Count + 2);
    }

    /// <summary>Replaces the path's vertices wholesale. Must contain at least 2 points.</summary>
    public void SetPoints(IReadOnlyList<Point> points)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(points.Count, 2, nameof(points));

        _points.Clear();
        _points.AddRange(points);
        UpdateBounds();
        _verticesChanged = true;
    }

    private void UpdateBounds()
    {
        float minX = _points[0].X;
        float maxX = _points[0].X;
        float minY = _points[0].Y;
        float maxY = _points[0].Y;

        for (int pointIndex = 1; pointIndex < _points.Count; pointIndex++)
        {
            minX = Math.Min(minX, _points[pointIndex].X);
            maxX = Math.Max(maxX, _points[pointIndex].X);
            minY = Math.Min(minY, _points[pointIndex].Y);
            maxY = Math.Max(maxY, _points[pointIndex].Y);
        }

        X = minX;
        Y = minY;
        Width = maxX - minX;
        Height = maxY - minY;
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
