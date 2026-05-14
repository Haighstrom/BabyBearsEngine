using System.Collections.Generic;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// Reconstructs the path produced by a graph-search algorithm. Assumes each node's
/// <see cref="IPathfindNode{TNode}.ParentNode"/> has been set as the algorithm explored.
/// </summary>
/// <typeparam name="TNode">Node type.</typeparam>
public interface IPathBuilder<TNode>
    where TNode : IPathfindNode<TNode>
{
    /// <summary>
    /// Walks back from <paramref name="end"/> via <see cref="IPathfindNode{TNode}.ParentNode"/>
    /// until reaching <paramref name="start"/>, returning the path in forward order
    /// (start at index 0, end at the last index).
    /// </summary>
    IList<TNode> BuildPath(TNode start, TNode end);
}
