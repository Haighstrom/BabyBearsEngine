namespace BabyBearsEngine.Pathfinding;

/// <summary>Lifecycle state of an <see cref="IPathSolver{TNode}"/>.</summary>
public enum SolveStatus
{
    /// <summary>The solver has been created but no steps have been taken.</summary>
    NotStarted,

    /// <summary>The solver has steps remaining; <see cref="IPathSolver{TNode}.Step"/> should be called again.</summary>
    InProgress,

    /// <summary>The solver reached the goal and the path is available.</summary>
    Success,

    /// <summary>The solver exhausted reachable nodes without finding a path.</summary>
    Failure,
}
