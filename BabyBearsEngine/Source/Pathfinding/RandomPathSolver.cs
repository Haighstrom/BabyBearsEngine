namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// Builds a random walk from <c>start</c> by repeatedly picking a passable neighbour until
/// the requested step count is reached (success) or the walk gets stuck (failure — typically
/// a dead end when <c>canBacktrack</c> is false).
/// </summary>
/// <typeparam name="TNode">Node type.</typeparam>
public sealed class RandomPathSolver<TNode> : IPathSolver<TNode>
    where TNode : IPathfindNode<TNode>
{
    private readonly Func<TNode, TNode, bool> _passableTest;
    private readonly int _targetSteps;
    private readonly bool _canBacktrack;
    private readonly Random _random = new();
    private List<TNode> _openNodes = [];
    private TNode _currentNode;
    private SolveStatus _state = SolveStatus.NotStarted;

    /// <param name="start">Node to start the walk from.</param>
    /// <param name="passableTest">Predicate testing whether the edge from one node to a connected one is traversable.</param>
    /// <param name="targetSteps">How many steps to take from the start node.</param>
    /// <param name="canBacktrack">If false, the walk avoids nodes already in the path (returns a path of unique nodes).</param>
    public RandomPathSolver(TNode start, Func<TNode, TNode, bool> passableTest, int targetSteps, bool canBacktrack)
    {
        _currentNode = start;
        Path = [start];
        _passableTest = passableTest;
        _targetSteps = targetSteps;
        _canBacktrack = canBacktrack;

        IdentifyOpenNodes();
    }

    /// <inheritdoc/>
    public (IList<TNode> ExploredNodes, IList<TNode> NodesToExplore, SolveStatus SolveStatus) State => (Path, _openNodes, _state);

    /// <inheritdoc/>
    public IList<TNode> Path { get; private set; }

    private void IdentifyOpenNodes()
    {
        _openNodes = _currentNode.ConnectedNodes
            .Where(n => _passableTest(_currentNode, n) && (!Path.Contains(n) || _canBacktrack))
            .ToList();
    }

    /// <inheritdoc/>
    public SolveStatus Step()
    {
        if (_openNodes.Count == 0)
        {
            return _state = SolveStatus.Failure;
        }

        var nextNode = _openNodes[_random.Next(_openNodes.Count)];
        Path.Add(nextNode);
        _currentNode = nextNode;

        IdentifyOpenNodes();

        if (Path.Count - 1 == _targetSteps)
        {
            return _state = SolveStatus.Success;
        }

        return _state = SolveStatus.InProgress;
    }

    /// <inheritdoc/>
    public bool TrySolve()
    {
        do
        {
            Step();
        }
        while (_state == SolveStatus.InProgress);

        return _state == SolveStatus.Success;
    }
}
