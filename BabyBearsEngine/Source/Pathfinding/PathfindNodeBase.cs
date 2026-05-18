using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// Base class for nodes that have a position. Equality is by position. Concrete subclasses
/// (e.g. <see cref="PathfindNode"/>, <see cref="PathfindNode{TEnum}"/>) add the connection
/// list and any per-node payload.
/// </summary>
/// <param name="x">X coordinate.</param>
/// <param name="y">Y coordinate.</param>
/// <param name="distanceBetweenConnectedNodes">Cost of moving to any directly-connected neighbour. Defaults to 1.</param>
public abstract class PathfindNodeBase(float x, float y, float distanceBetweenConnectedNodes = 1f) : IPosition
{

    /// <inheritdoc/>
    public float X { get; } = x;

    /// <inheritdoc/>
    public float Y { get; } = y;

    /// <summary>The cost of moving from this node to any directly-connected neighbour.</summary>
    public virtual float DistanceBetweenConnectedNodes { get; } = distanceBetweenConnectedNodes;

    /// <summary>Algorithm scratch data (e.g. A* uses this for F/G scores).</summary>
    public object? GraphSearchData { get; set; }

    /// <inheritdoc/>
    public bool Equals(IPosition? other)
    {
        if (other is null)
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    /// <inheritdoc/>
    public override bool Equals(object? other) => Equals(other as IPosition);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
