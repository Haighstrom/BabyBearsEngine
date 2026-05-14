namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A node that graph-search algorithms can operate on. Adds per-node bookkeeping the
/// algorithms need (distance to neighbours, the parent that discovered this node,
/// algorithm-specific scratch data).
/// </summary>
/// <typeparam name="TNode">The concrete node type (CRTP).</typeparam>
public interface IPathfindNode<TNode> : INode<TNode>
    where TNode : IPathfindNode<TNode>
{
    /// <summary>The cost of moving from this node to any directly-connected neighbour.</summary>
    float DistanceBetweenConnectedNodes { get; }

    /// <summary>The node a search algorithm reached this one from; used to reconstruct the path on completion.</summary>
    TNode? ParentNode { get; set; }

    /// <summary>Scratch data stored by a search algorithm (e.g. A* uses this for F/G scores).</summary>
    object? GraphSearchData { get; set; }
}
