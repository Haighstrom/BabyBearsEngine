namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// High-level pathfinder over a node graph. Wraps the underlying <see cref="IPathSolver{TNode}"/>
/// so callers can ask for a path without managing solver state.
/// </summary>
/// <typeparam name="TNode">Node type.</typeparam>
public interface IPathfinder<TNode>
    where TNode : IPathfindNode<TNode>
{
    /// <summary>Predicate testing whether the edge from one node to a connected one is traversable.</summary>
    delegate bool PassTestDelegate(TNode from, TNode to);

    /// <summary>
    /// Finds the lowest-cost path from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <returns>The path on success, or <c>null</c> if no path exists.</returns>
    IList<TNode>? FindPath(TNode start, TNode end, Func<TNode, TNode, bool> passableTest);

    /// <summary>
    /// Builds a random walk of up to <paramref name="targetSteps"/> steps from
    /// <paramref name="start"/>. If the walk runs out of moves (e.g. dead end without
    /// backtracking), the path returned is shorter than the target.
    /// </summary>
    /// <param name="start">Node to start the walk from.</param>
    /// <param name="passableTest">Edge passability test.</param>
    /// <param name="targetSteps">How many steps to attempt.</param>
    /// <param name="canBacktrack">If false, the walk avoids nodes already in the path.</param>
    IList<TNode> FindRandomPath(TNode start, Func<TNode, TNode, bool> passableTest, int targetSteps, bool canBacktrack);
}
