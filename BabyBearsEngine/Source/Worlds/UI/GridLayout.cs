using System.Collections.Generic;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A container entity that arranges children in a fixed grid of rows and columns.
/// Column and row sizes are specified via <see cref="GridCellSize.Fixed"/> or
/// <see cref="GridCellSize.Weighted"/>; the layout recalculates child positions
/// whenever the grid's own size changes.
/// </summary>
/// <remarks>
/// Children are placed explicitly via
/// <see cref="AddChild(IRectAddable, int, int, GridAlignment)"/> or sequentially
/// (left-to-right, top-to-bottom) via
/// <see cref="AddChild(IRectAddable, GridAlignment)"/>.
/// Use <see cref="RemoveChild"/> and <see cref="RemoveAllChildren"/> rather than
/// the base <c>Remove</c> / <c>RemoveAll</c> to keep the internal tracking in sync.
/// </remarks>
/// <param name="x">X position relative to the parent container.</param>
/// <param name="y">Y position relative to the parent container.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="columns">Size specification for each column, left to right.</param>
/// <param name="rows">Size specification for each row, top to bottom.</param>
/// <param name="padding">Uniform inset from all four edges in pixels.</param>
/// <param name="gap">Space between adjacent columns and between adjacent rows in pixels.</param>
public class GridLayout(float x, float y, float width, float height,
                  GridCellSize[] columns, GridCellSize[] rows,
                  float padding = 0f, float gap = 0f) : Entity(x, y, width, height)
{
    private readonly List<GridEntry> _entries = new();
    private int _autoCol = 0;
    private int _autoRow = 0;

    /// <summary>Number of columns defined in this grid.</summary>
    public int ColumnCount => columns.Length;

    /// <summary>Number of rows defined in this grid.</summary>
    public int RowCount => rows.Length;

    /// <summary>
    /// Adds a child at a specific cell. The child's position and size are immediately
    /// set according to <paramref name="alignment"/>.
    /// </summary>
    /// <param name="child">The child entity to add.</param>
    /// <param name="col">Zero-based column index.</param>
    /// <param name="row">Zero-based row index.</param>
    /// <param name="alignment">How the child is positioned within the cell.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="col"/> or <paramref name="row"/> is outside the grid bounds.
    /// </exception>
    public void AddChild(IRectAddable child, int col, int row,
                         GridAlignment alignment = GridAlignment.Fill)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(col);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(col, columns.Length);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, rows.Length);

        _entries.Add(new GridEntry(child, col, row, alignment));
        base.Add(child);
        UpdatePositions();
    }

    /// <summary>
    /// Adds a child at the next available cell, filling left-to-right then top-to-bottom.
    /// </summary>
    /// <param name="child">The child entity to add.</param>
    /// <param name="alignment">How the child is positioned within the cell.</param>
    /// <exception cref="InvalidOperationException">Thrown when all cells are already filled.</exception>
    public void AddChild(IRectAddable child, GridAlignment alignment = GridAlignment.Fill)
    {
        if (_autoRow >= rows.Length)
        {
            throw new InvalidOperationException("All grid cells are already filled.");
        }

        AddChild(child, _autoCol, _autoRow, alignment);
        AdvanceAutoPosition();
    }

    /// <summary>
    /// Removes a child that was added via <see cref="AddChild(IRectAddable, int, int, GridAlignment)"/>
    /// or <see cref="AddChild(IRectAddable, GridAlignment)"/>.
    /// </summary>
    public void RemoveChild(IRectAddable child)
    {
        _entries.RemoveAll(e => ReferenceEquals(e.Child, child));
        base.Remove(child);
    }

    /// <summary>Removes all children added via <c>AddChild</c> and resets the auto-placement cursor.</summary>
    public void RemoveAllChildren()
    {
        _entries.Clear();
        _autoCol = 0;
        _autoRow = 0;
        base.RemoveAll();
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        UpdatePositions();
    }

    /// <summary>
    /// Computes the pixel sizes for a sequence of column or row specs given the available
    /// space and gap between cells. Exposed internally for unit testing.
    /// </summary>
    internal static float[] ComputeSizes(GridCellSize[] specs, float available, float gap)
    {
        float spaceForCells = available - Math.Max(0, specs.Length - 1) * gap;

        float fixedTotal = 0f;
        float weightTotal = 0f;

        foreach (GridCellSize spec in specs)
        {
            if (spec.Kind == GridCellSizeKind.Fixed)
            {
                fixedTotal += spec.Value;
            }
            else
            {
                weightTotal += spec.Value;
            }
        }

        float weightedSpace = Math.Max(0f, spaceForCells - fixedTotal);

        float[] sizes = new float[specs.Length];
        for (int i = 0; i < specs.Length; i++)
        {
            sizes[i] = specs[i].Kind == GridCellSizeKind.Fixed
                ? specs[i].Value
                : weightTotal > 0f ? (specs[i].Value / weightTotal) * weightedSpace : 0f;
        }

        return sizes;
    }

    /// <summary>
    /// Computes the pixel offset of each cell given their sizes, the layout padding, and the gap.
    /// Exposed internally for unit testing.
    /// </summary>
    internal static float[] ComputeOffsets(float[] sizes, float padding, float gap)
    {
        float[] offsets = new float[sizes.Length];
        float cursor = padding;
        for (int i = 0; i < sizes.Length; i++)
        {
            offsets[i] = cursor;
            cursor += sizes[i] + gap;
        }
        return offsets;
    }

    private void AdvanceAutoPosition()
    {
        _autoCol++;
        if (_autoCol >= columns.Length)
        {
            _autoCol = 0;
            _autoRow++;
        }
    }

    private void UpdatePositions()
    {
        float[] colWidths = ComputeSizes(columns, Width - 2f * padding, gap);
        float[] rowHeights = ComputeSizes(rows, Height - 2f * padding, gap);
        float[] colX = ComputeOffsets(colWidths, padding, gap);
        float[] rowY = ComputeOffsets(rowHeights, padding, gap);

        foreach (GridEntry entry in _entries)
        {
            ApplyAlignment(entry.Child,
                           colX[entry.Col], rowY[entry.Row],
                           colWidths[entry.Col], rowHeights[entry.Row],
                           entry.Alignment);
        }
    }

    private static void ApplyAlignment(IRectAddable child,
                                       float cellX, float cellY,
                                       float cellW, float cellH,
                                       GridAlignment alignment)
    {
        var (hAlign, vAlign) = DecomposeAlignment(alignment);

        float childW = hAlign == HAlign.Fill ? cellW : child.Width;
        float childH = vAlign == VAlign.Fill ? cellH : child.Height;

        float x = hAlign switch
        {
            HAlign.Fill or HAlign.Left => cellX,
            HAlign.Center => cellX + (cellW - childW) / 2f,
            HAlign.Right => cellX + cellW - childW,
            _ => cellX,
        };

        float y = vAlign switch
        {
            VAlign.Fill or VAlign.Top => cellY,
            VAlign.Middle => cellY + (cellH - childH) / 2f,
            VAlign.Bottom => cellY + cellH - childH,
            _ => cellY,
        };

        child.X = x;
        child.Y = y;
        child.Width = childW;
        child.Height = childH;
    }

    private static (HAlign h, VAlign v) DecomposeAlignment(GridAlignment alignment) => alignment switch
    {
        GridAlignment.Fill                => (HAlign.Fill,   VAlign.Fill),
        GridAlignment.Center              => (HAlign.Center, VAlign.Middle),
        GridAlignment.TopLeft             => (HAlign.Left,   VAlign.Top),
        GridAlignment.TopCenter           => (HAlign.Center, VAlign.Top),
        GridAlignment.TopRight            => (HAlign.Right,  VAlign.Top),
        GridAlignment.MiddleLeft          => (HAlign.Left,   VAlign.Middle),
        GridAlignment.MiddleRight         => (HAlign.Right,  VAlign.Middle),
        GridAlignment.BottomLeft          => (HAlign.Left,   VAlign.Bottom),
        GridAlignment.BottomCenter        => (HAlign.Center, VAlign.Bottom),
        GridAlignment.BottomRight         => (HAlign.Right,  VAlign.Bottom),
        GridAlignment.StretchHorizontally => (HAlign.Fill,   VAlign.Middle),
        GridAlignment.StretchVertically   => (HAlign.Center, VAlign.Fill),
        _                                 => (HAlign.Fill,   VAlign.Fill),
    };

    private enum HAlign { Left, Center, Right, Fill }
    private enum VAlign { Top, Middle, Bottom, Fill }

    private readonly record struct GridEntry(IRectAddable Child, int Col, int Row, GridAlignment Alignment);
}
