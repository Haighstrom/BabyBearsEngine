using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Matrix3 = BabyBearsEngine.Geometry.Matrix3;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A tile map rendered from a sprite sheet. Each cell holds a zero-based frame index into the
/// sheet; setting a cell via the indexer marks the vertex buffer dirty and rebuilds it on the
/// next <see cref="Render"/> call. Construction allocates GL resources — must be created on
/// the engine thread.
/// </summary>
/// <param name="tiles">
/// Initial tile indices, indexed as [column, row]. Each value is a zero-based frame
/// index into <paramref name="texture"/>. The array is copied; later mutations must go
/// through the indexer.
/// </param>
/// <param name="tileW">On-screen width of each tile in pixels.</param>
/// <param name="tileH">On-screen height of each tile in pixels.</param>
/// <param name="texture">Sprite sheet texture (not owned; not disposed with this map).</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
public class SpriteMap(int[,] tiles, float tileW, float tileH, ISpriteTexture texture, float x = 0, float y = 0, int layer = int.MaxValue)
    : GraphicBase(x, y, tiles.GetLength(0) * tileW, tiles.GetLength(1) * tileH, layer), IGraphic, IDisposable
{
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly StandardMatrixShaderProgram _shader = new();
    private readonly int[,] _tiles = (int[,])tiles.Clone();
    private int _vertexCount = 0;
    private bool _verticesDirty = true;
    private Colour _colour = Colour.White;
    private Rect? _drawArea = null;
    private bool _disposed = false;

    /// <summary>Number of tile columns.</summary>
    public int Columns { get; } = tiles.GetLength(0);

    /// <inheritdoc cref="IGraphic.Colour"/>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesDirty = true;
        }
    }

    /// <summary>
    /// Restricts the vertex rebuild (and therefore the draw) to tiles whose own rect overlaps this area.
    /// The rect is in this <see cref="SpriteMap"/>'s local space — i.e. the same coordinate system as
    /// the tile grid itself, with (0, 0) at the top-left tile. Set to <c>null</c> (the default) to
    /// rebuild and draw every tile in the grid.
    /// </summary>
    /// <remarks>
    /// Setting this property marks the vertex buffer dirty, so each change triggers a rebuild on the
    /// next <see cref="Render"/> call. For a large mostly-static tile map this is a net win — the
    /// rebuild cost becomes proportional to visible tiles rather than total tiles — but for a small
    /// map being panned across constantly it can cost more than just rebuilding the whole grid.
    /// </remarks>
    public Rect? DrawArea
    {
        get => _drawArea;
        set
        {
            _drawArea = value;
            _verticesDirty = true;
        }
    }

    /// <summary>Number of tile rows.</summary>
    public int Rows { get; } = tiles.GetLength(1);

    /// <summary>Gets or sets the tile frame index at column <paramref name="col"/>, row <paramref name="row"/>.</summary>
    public int this[int col, int row]
    {
        get => _tiles[col, row];
        set
        {
            if (_tiles[col, row] != value)
            {
                _tiles[col, row] = value;
                _verticesDirty = true;
            }
        }
    }

    /// <summary>Sets every tile to <paramref name="frameIndex"/>.</summary>
    public void SetAll(int frameIndex)
    {
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                _tiles[col, row] = frameIndex;
            }
        }

        _verticesDirty = true;
    }

    /// <summary>Returns true if (col, row) is within the map bounds.</summary>
    public bool IsInBounds(int col, int row) => col >= 0 && col < Columns && row >= 0 && row < Rows;

    /// <inheritdoc/>
    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (Width == 0 || Height == 0)
        {
            return;
        }

        if (_verticesDirty)
        {
            RebuildVertices();
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);
        _shader.Bind();
        _vertexDataBuffer.Bind();
        texture.Bind();
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
    }

    private void RebuildVertices()
    {
        Color4 c = _colour.ToOpenTK();
        (int colMin, int colExclusiveMax, int rowMin, int rowExclusiveMax) =
            ComputeVisibleTileRange(_drawArea, Columns, Rows, tileW, tileH);
        int visibleColumns = colExclusiveMax - colMin;
        int visibleRows = rowExclusiveMax - rowMin;
        var vertices = new Vertex[visibleColumns * visibleRows * 6];
        int vi = 0;

        for (int col = colMin; col < colExclusiveMax; col++)
        {
            for (int row = rowMin; row < rowExclusiveMax; row++)
            {
                var (u1, v1, u2, v2) = texture.GetFrameUVs(_tiles[col, row]);
                float x1 = col * tileW;
                float y1 = row * tileH;
                float x2 = x1 + tileW;
                float y2 = y1 + tileH;

                // Two clockwise triangles forming the tile quad.
                vertices[vi++] = new Vertex(x1, y1, c, u1, v1); // TL
                vertices[vi++] = new Vertex(x2, y1, c, u2, v1); // TR
                vertices[vi++] = new Vertex(x1, y2, c, u1, v2); // BL
                vertices[vi++] = new Vertex(x2, y1, c, u2, v1); // TR
                vertices[vi++] = new Vertex(x2, y2, c, u2, v2); // BR
                vertices[vi++] = new Vertex(x1, y2, c, u1, v2); // BL
            }
        }

        _vertexDataBuffer.SetNewVertices(vertices);
        _vertexCount = vi;
        _verticesDirty = false;
    }

    /// <summary>
    /// Computes the half-open tile-index range that overlaps <paramref name="drawArea"/>. When
    /// <paramref name="drawArea"/> is null, returns the full grid. The returned ranges are
    /// clamped to <c>[0, columns]</c> and <c>[0, rows]</c>; if the area falls outside the grid
    /// or is empty, the returned range is empty (max == min).
    /// </summary>
    internal static (int colMin, int colExclusiveMax, int rowMin, int rowExclusiveMax) ComputeVisibleTileRange(
        Rect? drawArea,
        int columns,
        int rows,
        float tileWidth,
        float tileHeight)
    {
        if (drawArea is null)
        {
            return (0, columns, 0, rows);
        }

        if (drawArea.W <= 0 || drawArea.H <= 0)
        {
            return (0, 0, 0, 0);
        }

        int colMin = Math.Clamp((int)Math.Floor(drawArea.Left / tileWidth), 0, columns);
        int colExclusiveMax = Math.Clamp((int)Math.Ceiling(drawArea.Right / tileWidth), 0, columns);
        int rowMin = Math.Clamp((int)Math.Floor(drawArea.Top / tileHeight), 0, rows);
        int rowExclusiveMax = Math.Clamp((int)Math.Ceiling(drawArea.Bottom / tileHeight), 0, rows);

        if (colExclusiveMax <= colMin || rowExclusiveMax <= rowMin)
        {
            return (0, 0, 0, 0);
        }

        return (colMin, colExclusiveMax, rowMin, rowExclusiveMax);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _vertexDataBuffer.Dispose();
            _shader.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
