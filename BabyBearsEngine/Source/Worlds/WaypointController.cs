using System.Collections.Generic;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Moves an <see cref="IWaypointable"/> target along an ordered list of waypoints.
/// Add this controller to the scene graph alongside the target so it receives
/// <see cref="Update"/> calls each frame.
/// </summary>
public class WaypointController(IWaypointable target) : UpdateableBase, IWaypointController
{
    private Direction? _lastDirection = null;

    public bool ReachedDestination => Waypoints.Count == 0;

    public IList<IPosition> Waypoints { get; } = new List<IPosition>();

    public event EventHandler? Arrived;

    public event EventHandler<DirectionChangedEventArgs>? DirectionChanged;

    public event EventHandler? ReachedWaypoint;

    public void AddWaypoints(IEnumerable<IPosition> waypoints)
    {
        foreach (IPosition wp in waypoints)
        {
            Waypoints.Add(wp);
        }
    }

    public void AddWaypoints(params IPosition[] waypoints) => AddWaypoints((IEnumerable<IPosition>)waypoints);

    public void ClearWaypoints()
    {
        Waypoints.Clear();
    }

    public IPosition GetNextWaypoint()
    {
        if (Waypoints.Count == 0)
        {
            throw new InvalidOperationException("No waypoints have been set.");
        }

        return Waypoints[0];
    }

    public void SetWaypoints(IEnumerable<IPosition> waypoints)
    {
        Waypoints.Clear();

        foreach (IPosition wp in waypoints)
        {
            Waypoints.Add(wp);
        }
    }

    public void SetWaypoints(params IPosition[] waypoints) => SetWaypoints((IEnumerable<IPosition>)waypoints);

    public override void Update(double elapsed)
    {
        if (ReachedDestination)
        {
            return;
        }

        // Total distance the target can travel this frame.
        float amountToMove = (float)(elapsed * target.Speed);
        Direction? currentDirection = null;

        // Each iteration consumes one waypoint or exhausts the movement budget.
        // High-speed entities can reach (and pass through) multiple waypoints per frame.
        while (amountToMove > 0 && Waypoints.Count > 0)
        {
            IPosition next = Waypoints[0];
            Point delta = new(next.X - target.X, next.Y - target.Y);
            float distance = delta.Length;

            // Already standing on this waypoint (e.g. spawned exactly there); skip it.
            if (distance == 0)
            {
                Waypoints.RemoveAt(0);
                ReachedWaypoint?.Invoke(this, EventArgs.Empty);
                continue;
            }

            currentDirection = delta.ToDirection();

            if (distance > amountToMove)
            {
                // Waypoint is further than this frame's budget: move as far as possible
                // along the unit vector toward it and stop.
                Point normal = delta.Normal;
                target.X += normal.X * amountToMove;
                target.Y += normal.Y * amountToMove;
                amountToMove = 0;
            }
            else
            {
                // Waypoint is within reach: snap to it exactly (avoids floating-point
                // drift), deduct the distance from the budget, then loop to consume
                // any further waypoints reachable with the remaining budget.
                target.X = next.X;
                target.Y = next.Y;
                amountToMove -= distance;
                Waypoints.RemoveAt(0);
                ReachedWaypoint?.Invoke(this, EventArgs.Empty);
            }
        }

        // Fire DirectionChanged once per frame (after the loop) rather than on every
        // waypoint, so a target that changes direction multiple times during one frame
        // raises an event only for the final direction change.
        if (currentDirection != _lastDirection && currentDirection is not null)
        {
            DirectionChanged?.Invoke(this, new DirectionChangedEventArgs(_lastDirection, currentDirection.Value));
        }

        if (ReachedDestination)
        {
            Arrived?.Invoke(this, EventArgs.Empty);
            // Reset so DirectionChanged fires again when the next journey begins.
            currentDirection = null;
        }

        _lastDirection = currentDirection;
    }
}
