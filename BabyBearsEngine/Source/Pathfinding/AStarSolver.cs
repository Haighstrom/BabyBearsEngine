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
    // Mutable per-node scratch holder. Allocated once the first time a node is seen and then
    // mutated in place on each relaxation, rather than allocating a fresh record per visit. F is
    // the priority used to order the open set; G is the accumulated cost from the start node.
    private sealed class AStarData
    {
        public float F { get; set; } = 0f;
        public float G { get; set; } = 0f;
    }

    private readonly TNode _start;
    private readonly TNode _end;
    private readonly Func<TNode, TNode, bool> _passableTest;
    private readonly Func<TNode, TNode, float> _heuristic;
    // Min-heap of candidate nodes keyed by F. A node may appear more than once after its F is
    // lowered; the stale (higher-F) entries are discarded on dequeue via _openSet membership
    // (lazy deletion), which is far cheaper than a decrease-key or re-sorting the whole open set.
    private readonly PriorityQueue<TNode, float> _openQueue = new();
    private readonly HashSet<TNode> _openSet = [];
    private readonly HashSet<TNode> _closedSet = [];
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
        _currentNode.GraphSearchData = new AStarData { F = 0f, G = 0f };
        _end = end;
        _passableTest = passableTest;
        _heuristic = heuristic;

        IdentifyOpenNodes();
    }

    /// <inheritdoc/>
    public (IList<TNode> ExploredNodes, IList<TNode> NodesToExplore, SolveStatus SolveStatus) State => ([.. _closedSet], [.. _openSet], _state);

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

            // Symmetric edge cost: average the source and destination node costs so a swamp tile is equally
            // expensive to enter from grass as it is to leave onto grass. Using only currentNode's cost
            // makes A→Z and Z→A produce different total costs when start and end have different costs.
            float edgeCost = (_currentNode.DistanceBetweenConnectedNodes + testNode.DistanceBetweenConnectedNodes) * 0.5f;
            float g = ((AStarData)_currentNode.GraphSearchData!).G + edgeCost;
            float h = _heuristic(testNode, _end);
            float f = g + h;

            bool inOpen = _openSet.Contains(testNode);
            bool unseen = !inOpen && !_closedSet.Contains(testNode);

            AStarData? data = (AStarData?)testNode.GraphSearchData;

            if (unseen || f < data!.F)
            {
                if (data is null)
                {
                    data = new AStarData();
                    testNode.GraphSearchData = data;
                }

                data.F = f;
                data.G = g;
                testNode.ParentNode = _currentNode;

                if (unseen)
                {
                    _openSet.Add(testNode);
                    _openQueue.Enqueue(testNode, f);
                }
                else if (inOpen)
                {
                    // F lowered for a node still queued — push a fresh entry at the better priority.
                    // The earlier, higher-priority entry is skipped via lazy deletion when dequeued.
                    _openQueue.Enqueue(testNode, f);
                }
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

        _closedSet.Add(_currentNode);

        // Pop the lowest-F genuinely-open node, discarding stale duplicate entries left behind when
        // a node's F was lowered (lazy deletion). An exhausted queue means no path remains.
        while (true)
        {
            if (!_openQueue.TryDequeue(out TNode? candidate, out _))
            {
                return _state = SolveStatus.Failure;
            }

            if (_openSet.Remove(candidate))
            {
                _currentNode = candidate;
                break;
            }
        }

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
