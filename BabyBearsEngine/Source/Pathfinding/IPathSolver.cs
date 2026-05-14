using System.Collections.Generic;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A graph-search algorithm that can be driven step-by-step (for visualisation or incremental
/// search) or all-at-once via <see cref="TrySolve"/>.
/// </summary>
/// <typeparam name="TNode">Node type.</typeparam>
public interface IPathSolver<TNode>
    where TNode : IPathfindNode<TNode>
{
    /// <summary>
    /// The current path. Empty if the solver hasn't started or failed to find a path.
    /// </summary>
    IList<TNode> Path { get; }

    /// <summary>
    /// Snapshot of the solver's state — nodes already evaluated, nodes queued for evaluation,
    /// and the current solve status.
    /// </summary>
    (IList<TNode> ExploredNodes, IList<TNode> NodesToExplore, SolveStatus SolveStatus) State { get; }

    /// <summary>
    /// Take a single step. Useful for incremental rendering / debugging. Returns the new
    /// <see cref="SolveStatus"/>.
    /// </summary>
    SolveStatus Step();

    /// <summary>
    /// Run the solver to completion. Returns <c>true</c> if a path was found.
    /// </summary>
    bool TrySolve();
}
