using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A rectangular 2D grid of pathfinding nodes. Constructs the grid via a factory,
/// wires each cell to its four 4-connected neighbours, and provides indexing and
/// nearest-node lookup. Inherits A* + random-path search from
/// <see cref="Pathfinder{TNode}"/>.
/// </summary>
/// <typeparam name="TNode">Node type. Must be a <see cref="IPathfindNode{TNode}"/> with a position.</typeparam>
public class GridPathfinder<TNode> : Pathfinder<TNode>, IGridPathfinder<TNode>
    where TNode : IPathfindNode<TNode>, IPosition
{
    /// <param name="width">Number of cells along X.</param>
    /// <param name="height">Number of cells along Y.</param>
    /// <param name="createNode">Factory called once per (x, y) cell to build the node stored there.</param>
    public GridPathfinder(int width, int height, Func<int, int, TNode> createNode)
    {
        Nodegrid = new TNode[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Nodegrid[i, j] = createNode(i, j);
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (IsValidGridPosition(i - 1, j))
                {
                    Nodegrid[i, j].ConnectedNodes.Add(Nodegrid[i - 1, j]);
                }
                if (IsValidGridPosition(i + 1, j))
                {
                    Nodegrid[i, j].ConnectedNodes.Add(Nodegrid[i + 1, j]);
                }
                if (IsValidGridPosition(i, j - 1))
                {
                    Nodegrid[i, j].ConnectedNodes.Add(Nodegrid[i, j - 1]);
                }
                if (IsValidGridPosition(i, j + 1))
                {
                    Nodegrid[i, j].ConnectedNodes.Add(Nodegrid[i, j + 1]);
                }
            }
        }
    }

    /// <summary>The backing array; subclasses may need direct access for bulk operations.</summary>
    protected TNode[,] Nodegrid { get; }

    /// <inheritdoc/>
    public TNode this[int x, int y] => Nodegrid[x, y];

    /// <inheritdoc/>
    public int Width => Nodegrid.GetLength(0);

    /// <inheritdoc/>
    public int Height => Nodegrid.GetLength(1);

    /// <inheritdoc/>
    public TNode GetClosestNode(float x, float y)
    {
        int cx = (int)Math.Clamp(x, 0, Width - 1);
        int cy = (int)Math.Clamp(y, 0, Height - 1);
        return this[cx, cy];
    }

    /// <inheritdoc/>
    public TNode GetClosestNode(IPosition position) => GetClosestNode(position.X, position.Y);

    /// <inheritdoc/>
    public bool IsValidGridPosition(float x, float y)
    {
        if (x % 1 != 0 || y % 1 != 0)
        {
            return false;
        }

        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
