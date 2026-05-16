using System.Collections.Generic;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Controls the movement of an <see cref="IWaypointable"/> entity along an ordered list of
/// positions. Each <see cref="IUpdateable.Update"/> advances the target by
/// <c>elapsed × Speed</c> pixels, consuming waypoints as they are reached.
/// </summary>
public interface IWaypointController : IAddable, IUpdateable
{
    /// <summary><c>true</c> when the waypoint list is empty (the target has arrived or no path was set).</summary>
    bool ReachedDestination { get; }

    /// <summary>The ordered list of positions remaining on the current path.</summary>
    IList<IPosition> Waypoints { get; }

    /// <summary>Raised when the last waypoint is consumed and the target reaches its final destination.</summary>
    event EventHandler? Arrived;

    /// <summary>Raised when the direction of travel changes during movement.</summary>
    event EventHandler<DirectionChangedEventArgs>? DirectionChanged;

    /// <summary>Raised each time an individual waypoint is consumed (including the last one, before <see cref="Arrived"/>).</summary>
    event EventHandler? ReachedWaypoint;

    /// <summary>Appends waypoints to the end of the current path.</summary>
    void AddWaypoints(IEnumerable<IPosition> waypoints);

    /// <inheritdoc cref="AddWaypoints(IEnumerable{IPosition})"/>
    void AddWaypoints(params IPosition[] waypoints);

    /// <summary>Removes all remaining waypoints. Does not raise any events.</summary>
    void ClearWaypoints();

    /// <summary>Returns the next waypoint without removing it.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the waypoint list is empty.</exception>
    IPosition GetNextWaypoint();

    /// <summary>Replaces the current path with the supplied waypoints.</summary>
    void SetWaypoints(IEnumerable<IPosition> waypoints);

    /// <inheritdoc cref="SetWaypoints(IEnumerable{IPosition})"/>
    void SetWaypoints(params IPosition[] waypoints);
}
