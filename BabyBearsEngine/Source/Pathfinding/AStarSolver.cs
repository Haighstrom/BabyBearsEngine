namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A* graph-search solver. Finds the lowest-cost path from <c>start</c> to <c>end</c>
/// using a caller-supplied passable test (which edges are traversable) and heuristic
/// (estimate of cost to reach the goal from a candidate node).
/// </summary>
/// <typeparam name="TNode">Node type.</typeparam>
public sealed class AStarSolver<TNode> : IPathSolver<TNode>
    where TNode : IPathfindNode<TNode>
{
    private sealed record AStarData(float F, float G);

    private readonly TNode _start;
    private readonly TNode _end;
    private readonly Func<TNode, TNode, bool> _passableTest;
    private readonly Func<TNode, TNode, float> _heuristic;
    private readonly List<TNode> _openNodes = [];
    private readonly List<TNode> _closedNodes = [];
    private readonly IPathBuilder<TNode> _pathBuilder = new PathBuilder<TNode>();
    private TNode _currentNode;
    private SolveStatus _state = SolveStatus.NotStarted;

    /// <param name="start">Node to start searching from.</param>
    /// <param name="end">Goal node.</param>
    /// <param name="passableTest">Predicate testing whether the edge from one node to a connected one is traversable.</param>
    /// <param name="heuristic">Heuristic function estimating the remaining cost from a candidate node to the goal. Manhattan distance is a common choice.</param>
    public AStarSolver(TNode start, TNode end, Func<TNode, TNode, bool> passableTest, Func<TNode, TNode, float> heuristic)
    {
        _currentNode = _start = start;
        _currentNode.GraphSearchData = new AStarData(F: 0, G: 0);
        _end = end;
        _passableTest = passableTest;
        _heuristic = heuristic;

        IdentifyOpenNodes();
    }

    /// <inheritdoc/>
    public (IList<TNode> ExploredNodes, IList<TNode> NodesToExplore, SolveStatus SolveStatus) State => (_closedNodes, _openNodes, _state);

    /// <inheritdoc/>
    public IList<TNode> Path => _pathBuilder.BuildPath(_start, _currentNode);

    private void IdentifyOpenNodes()
    {
        foreach (TNode testNode in _currentNode.ConnectedNodes)
        {
            if (!_passableTest(_currentNode, testNode))
            {
                continue;
            }

            float g = ((AStarData)_currentNode.GraphSearchData!).G + _currentNode.DistanceBetweenConnectedNodes;
            float h = _heuristic(testNode, _end);
            float f = g + h;

            bool unseen = !_openNodes.Contains(testNode) && !_closedNodes.Contains(testNode);

            if (unseen || f < ((AStarData)testNode.GraphSearchData!).F)
            {
                testNode.GraphSearchData = new AStarData(F: f, G: g);
                testNode.ParentNode = _currentNode;
            }

            if (unseen)
            {
                _openNodes.Add(testNode);
            }
        }
    }

    /// <inheritdoc/>
    public SolveStatus Step()
    {
        if (_start.Equals(_end))
        {
            return _state = SolveStatus.Success;
        }

        _closedNodes.Add(_currentNode);

        if (_openNodes.Count == 0)
        {
            return _state = SolveStatus.Failure;
        }

        // TODO: priority queue. For now we sort the open list each step.
        _openNodes.Sort((n1, n2) => ((AStarData)n2.GraphSearchData!).F.CompareTo(((AStarData)n1.GraphSearchData!).F));
        _currentNode = _openNodes[^1];
        _openNodes.RemoveAt(_openNodes.Count - 1);

        IdentifyOpenNodes();

        if (_currentNode.Equals(_end))
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
