using System.Collections.Generic;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A plain pathfinding node — has a position, a list of connected nodes, and the per-search
/// scratch slots. Equality is by position (inherited from <see cref="PathfindNodeBase"/>).
/// </summary>
/// <param name="x">X coordinate.</param>
/// <param name="y">Y coordinate.</param>
/// <param name="distanceBetweenConnectedNodes">Cost of moving to a directly-connected neighbour.</param>
public class PathfindNode(float x, float y, float distanceBetweenConnectedNodes = 1f) : PathfindNodeBase(x, y, distanceBetweenConnectedNodes), IPathfindNode<PathfindNode>, IPosition
{

    /// <inheritdoc/>
    public IList<PathfindNode> ConnectedNodes { get; } = [];

    /// <inheritdoc/>
    public PathfindNode? ParentNode { get; set; }

    /// <inheritdoc/>
    public bool Equals(PathfindNode? other) => Equals(other as IPosition);
}
