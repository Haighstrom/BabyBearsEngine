using System.Collections.Generic;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// Default <see cref="IPathBuilder{TNode}"/>: walks <see cref="IPathfindNode{TNode}.ParentNode"/>
/// backwards from end to start, returning the path in forward order.
/// </summary>
internal sealed class PathBuilder<TNode> : IPathBuilder<TNode>
    where TNode : IPathfindNode<TNode>
{
    /// <inheritdoc/>
    public IList<TNode> BuildPath(TNode start, TNode end)
    {
        TNode current = end;
        List<TNode> path = [current];

        while (!current.Equals(start))
        {
            current = current.ParentNode ?? throw new InvalidOperationException("Path could not be built — ParentNode chain was broken before reaching the start node.");
            path.Insert(0, current);
        }

        return path;
    }
}
