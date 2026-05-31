using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// Walks the entity attached to <paramref name="controller"/> through a pre-computed list
/// of <paramref name="waypoints"/> and completes when the controller fires its
/// <see cref="IWaypointController.Arrived"/> event.
/// </summary>
/// <remarks>
/// Use this when the caller already knows the path (scripted route, hand-authored patrol,
/// path computed once and re-walked). For pathfind-then-walk in one step, use
/// <see cref="PathfindToTask{TNode}"/>.
/// </remarks>
/// <param name="controller">Waypoint controller for the entity being moved.</param>
/// <param name="waypoints">Ordered path through world space.</param>
public class FollowPathTask(IWaypointController controller, IEnumerable<IPosition> waypoints) : Task
{
    private bool _arrived = false;

    /// <inheritdoc/>
    public override bool IsComplete => _arrived;

    /// <inheritdoc/>
    public override void Complete()
    {
        controller.Arrived -= OnArrived;
        base.Complete();
    }

    /// <inheritdoc/>
    public override void Start()
    {
        controller.Arrived += OnArrived;
        controller.SetWaypoints(waypoints);
        base.Start();
    }

    /// <inheritdoc/>
    protected override void OnCancel()
    {
        controller.Arrived -= OnArrived;

        if (!_arrived)
        {
            controller.ClearWaypoints();
        }
    }

    /// <inheritdoc/>
    protected override void OnReset()
    {
        controller.Arrived -= OnArrived;
        _arrived = false;
    }

    private void OnArrived(object? sender, EventArgs e)
    {
        _arrived = true;
    }
}
