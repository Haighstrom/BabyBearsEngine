using BabyBearsEngine.Geometry;
using BabyBearsEngine.Pathfinding;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Controller that pathfinds across an <see cref="IPathfinder{TNode}"/> node graph and drives
/// an entity along the resulting path. Composes an inner <see cref="IWaypointController"/> —
/// the pathing controller computes the route; the waypoint controller moves the entity along it.
/// </summary>
/// <typeparam name="TNode">Node type stored in the pathfinder.</typeparam>
public interface IPathingController<TNode> : IAddable, IUpdateable
    where TNode : IPathfindNode<TNode>, IPosition
{
    /// <summary><c>true</c> when there are waypoints remaining on the current path.</summary>
    bool HasPath { get; }

    /// <summary>Predicate testing whether the edge from one node to a connected one is traversable.</summary>
    Func<TNode, TNode, bool> PassableTest { get; set; }

    /// <summary>Raised when the entity reaches the end of its current path. Proxied from the inner waypoint controller.</summary>
    event EventHandler? Arrived;

    /// <summary>Raised by <see cref="TryFindPath"/> when a route exists.</summary>
    event EventHandler? PathFound;

    /// <summary>Raised by <see cref="TryFindPath"/> when no route exists.</summary>
    event EventHandler? PathNotFound;

    /// <summary>Discards the current path. Does not raise events.</summary>
    void ClearPath();

    /// <summary>
    /// Pathfinds from <paramref name="start"/> to <paramref name="goal"/> and, on success,
    /// replaces the inner waypoint controller's path with the result.
    /// </summary>
    /// <returns><c>true</c> if a path was found and loaded; <c>false</c> otherwise.</returns>
    bool TryFindPath(TNode start, TNode goal);
}
