using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// Default <see cref="IPathfinder{TNode}"/>. Uses <see cref="AStarSolver{TNode}"/> for
/// goal-directed paths and <see cref="RandomPathSolver{TNode}"/> for random walks. The
/// A* heuristic is Manhattan distance by default; replace via <see cref="Heuristic"/>.
/// </summary>
/// <typeparam name="TNode">Node type. Must expose a position so the default heuristic can measure distance.</typeparam>
public class Pathfinder<TNode> : IPathfinder<TNode>
    where TNode : IPathfindNode<TNode>, IPosition
{
    /// <summary>Manhattan distance — sensible default for grid graphs.</summary>
    protected static readonly Func<TNode, TNode, float> DefaultHeuristic =
        (n1, n2) => Math.Abs(n1.X - n2.X) + Math.Abs(n1.Y - n2.Y);

    /// <summary>The A* heuristic. Defaults to <see cref="DefaultHeuristic"/> (Manhattan distance).</summary>
    public Func<TNode, TNode, float> Heuristic { get; set; } = DefaultHeuristic;

    /// <inheritdoc/>
    public IList<TNode>? FindPath(TNode start, TNode end, Func<TNode, TNode, bool> passableTest)
    {
        AStarSolver<TNode> solver = new(start, end, passableTest, Heuristic);

        return solver.TrySolve() ? solver.Path : null;
    }

    /// <inheritdoc/>
    public IList<TNode> FindRandomPath(TNode start, Func<TNode, TNode, bool> passableTest, int targetSteps, bool canBacktrack)
    {
        RandomPathSolver<TNode> solver = new(start, passableTest, targetSteps, canBacktrack);
        solver.TrySolve();
        return solver.Path;
    }
}
