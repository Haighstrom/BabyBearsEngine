using System.Collections.Generic;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A pathfinding node tagged with a <typeparamref name="TEnum"/>-valued "pass type" — useful
/// for terrain categories (Grass, Water, Wall, …) that the passable-test can dispatch on.
/// </summary>
/// <typeparam name="TEnum">Enum type identifying the node's terrain or category.</typeparam>
public class PathfindNode<TEnum> : IPathfindNode<PathfindNode<TEnum>>, IPosition
    where TEnum : Enum
{
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="passType">The node's terrain/category, used by passable-tests.</param>
    public PathfindNode(float x, float y, TEnum passType)
    {
        X = x;
        Y = y;
        PassType = passType;
    }

    /// <inheritdoc/>
    public float X { get; }

    /// <inheritdoc/>
    public float Y { get; }

    /// <inheritdoc/>
    public IList<PathfindNode<TEnum>> ConnectedNodes { get; } = [];

    /// <inheritdoc/>
    public virtual float DistanceBetweenConnectedNodes { get; } = 1f;

    /// <inheritdoc/>
    public object? GraphSearchData { get; set; }

    /// <inheritdoc/>
    public PathfindNode<TEnum>? ParentNode { get; set; }

    /// <summary>The node's terrain/category. Mutable so the world can change terrain at runtime.</summary>
    public TEnum PassType { get; set; }

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
    public bool Equals(PathfindNode<TEnum>? other) => Equals(other as IPosition);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <inheritdoc/>
    public override string ToString() => $"Node:({X},{Y})";
}
