using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A pathfinder backed by a 2D grid of nodes. Adds indexing, bounds checks, and
/// nearest-node lookup on top of the generic <see cref="IPathfinder{TNode}"/>.
/// </summary>
/// <typeparam name="TNode">Node type stored in the grid.</typeparam>
public interface IGridPathfinder<TNode> : IPathfinder<TNode>
    where TNode : IPathfindNode<TNode>, IPosition
{
    /// <summary>Grid width in cells.</summary>
    int Width { get; }

    /// <summary>Grid height in cells.</summary>
    int Height { get; }

    /// <summary>Node at grid coordinates (<paramref name="x"/>, <paramref name="y"/>).</summary>
    TNode this[int x, int y] { get; }

    /// <summary>True if (<paramref name="x"/>, <paramref name="y"/>) is an integer cell inside the grid.</summary>
    bool IsValidGridPosition(float x, float y);

    /// <summary>Returns the node closest to (<paramref name="x"/>, <paramref name="y"/>), clamped to the grid bounds.</summary>
    TNode GetClosestNode(float x, float y);

    /// <summary>Returns the node closest to <paramref name="position"/>, clamped to the grid bounds.</summary>
    TNode GetClosestNode(IPosition position);
}
