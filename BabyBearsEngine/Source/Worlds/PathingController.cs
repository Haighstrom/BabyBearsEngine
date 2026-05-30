using BabyBearsEngine.Geometry;
using BabyBearsEngine.Pathfinding;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Default <see cref="IPathingController{TNode}"/>. Owns an inner
/// <see cref="IWaypointController"/> — the pathing controller computes the route
/// via <paramref name="pathfinder"/>; the waypoint controller moves the entity along it.
/// </summary>
/// <remarks>
/// <para>The inner waypoint controller is driven only via this controller's
/// <see cref="Update"/>; do not add it to the scene tree separately, or its
/// <c>Update</c> will be called twice per frame.</para>
/// <para>Pathfinding is synchronous — <see cref="TryFindPath"/> blocks until the search
/// completes. Suitable for small/medium grids; for large worlds consider running on a
/// background thread at the call site.</para>
/// </remarks>
/// <param name="pathfinder">Pathfinder used to compute routes.</param>
/// <param name="waypointController">Waypoint controller that consumes the path and moves the entity.</param>
/// <param name="passableTest">Predicate testing whether the edge from one node to a connected one is traversable.</param>
/// <typeparam name="TNode">Node type stored in the pathfinder.</typeparam>
public class PathingController<TNode>(
    IPathfinder<TNode> pathfinder,
    IWaypointController waypointController,
    Func<TNode, TNode, bool> passableTest)
    : UpdateableBase, IPathingController<TNode>
    where TNode : IPathfindNode<TNode>, IPosition
{
    private EventHandler? _arrived;
    private bool _subscribed = false;

    /// <inheritdoc/>
    public bool HasPath => !waypointController.ReachedDestination;

    /// <inheritdoc/>
    public Func<TNode, TNode, bool> PassableTest { get; set; } = passableTest;

    /// <inheritdoc/>
    public event EventHandler? Arrived
    {
        add
        {
            _arrived += value;
            if (!_subscribed)
            {
                waypointController.Arrived += OnInnerArrived;
                _subscribed = true;
            }
        }
        remove
        {
            _arrived -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler? PathFound;

    /// <inheritdoc/>
    public event EventHandler? PathNotFound;

    /// <inheritdoc/>
    public void ClearPath()
    {
        waypointController.ClearWaypoints();
    }

    /// <inheritdoc/>
    public bool TryFindPath(TNode start, TNode goal)
    {
        IList<TNode>? path = pathfinder.FindPath(start, goal, PassableTest);

        if (path is null)
        {
            PathNotFound?.Invoke(this, EventArgs.Empty);
            return false;
        }

        waypointController.SetWaypoints(path.Cast<IPosition>());
        PathFound?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        waypointController.Update(elapsed);
    }

    private void OnInnerArrived(object? sender, EventArgs e)
    {
        _arrived?.Invoke(this, e);
    }
}
