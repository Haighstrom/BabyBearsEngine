using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A filled simple polygon (convex or concave, non-self-intersecting) through a list of boundary
/// points, triangulated via <see cref="PolygonTriangulator"/>. The loop closes automatically —
/// pass the same point as both the first and last entry, or leave it open; either way the closing
/// edge between the last and first point is implied. Construction allocates GL resources (vertex
/// buffer) — must be created on the engine thread after the GL context exists. Implements
/// <see cref="IDisposable"/> to release those resources.
/// </summary>
public sealed class PolygonGraphic : GraphicBase, IGraphic, IColourGraphic, IDisposable
{
    private readonly List<Point> _points;
    private readonly SolidColourShaderProgramMatrix _shader = Shaders.SolidColour;
    private readonly VertexDataBuffer<VertexNoTexture> _vertexDataBuffer = new();
    private Colour _colour;
    private int _triangleVertexCount = 0;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <param name="points">
    /// The polygon's boundary vertices, in the parent's local space. Must contain at least 3
    /// distinct points (after any repeated closing point is stripped). Pass the same point as
    /// both the first and last entry to close the loop explicitly, or leave it open.
    /// </param>
    /// <param name="colour">Fill colour.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public PolygonGraphic(IReadOnlyList<Point> points, Colour colour, int layer = int.MaxValue)
        : base(layer)
    {
        _points = NormalizeClosedPoints(points);
        _colour = colour;

        UpdateBounds();
    }

    /// <inheritdoc/>
    public float Angle { get; set; } = 0f;

    /// <summary>Fill colour.</summary>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <summary>The polygon's current boundary vertices, in the parent's local space.</summary>
    public IReadOnlyList<Point> Points => _points;

    private VertexNoTexture[] BuildVertices()
    {
        Point[] triangles = PolygonTriangulator.Triangulate(_points);
        var colourTK = _colour.ToOpenTK();

        VertexNoTexture[] vertices = new VertexNoTexture[triangles.Length];
        for (int vertexIndex = 0; vertexIndex < triangles.Length; vertexIndex++)
        {
            vertices[vertexIndex] = new VertexNoTexture(triangles[vertexIndex].X - X, triangles[vertexIndex].Y - Y, colourTK);
        }

        _triangleVertexCount = vertices.Length;
        return vertices;
    }

    private static List<Point> NormalizeClosedPoints(IReadOnlyList<Point> points)
    {
        List<Point> normalized = [.. points];
        if (normalized.Count > 1 && normalized[0] == normalized[^1])
        {
            normalized.RemoveAt(normalized.Count - 1);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(normalized.Count, 3, nameof(points));
        return normalized;
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

        GL.DrawArrays(PrimitiveType.Triangles, 0, _triangleVertexCount);
    }

    /// <summary>Replaces the polygon's vertices wholesale. Must contain at least 3 distinct points.</summary>
    public void SetPoints(IReadOnlyList<Point> points)
    {
        List<Point> normalized = NormalizeClosedPoints(points);
        _points.Clear();
        _points.AddRange(normalized);
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
