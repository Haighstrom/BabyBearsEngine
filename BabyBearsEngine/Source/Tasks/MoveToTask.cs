using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// Sends the entity attached to <paramref name="controller"/> straight to <paramref name="target"/>
/// and completes when the controller fires its <see cref="IWaypointController.Arrived"/> event.
/// </summary>
/// <remarks>
/// The task does not move the entity itself — it sets the controller's waypoint on
/// <see cref="Start"/> and observes the existing controller's per-frame movement. The
/// caller is responsible for adding the <see cref="IWaypointController"/> to the entity
/// (or a parent) so it receives its own <see cref="IUpdateable.Update"/> calls.
/// </remarks>
/// <param name="controller">Waypoint controller for the entity being moved.</param>
/// <param name="target">Destination in world space.</param>
public class MoveToTask(IWaypointController controller, IPosition target) : Task
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
        controller.SetWaypoints(target);
        base.Start();
    }

    /// <inheritdoc/>
    protected override void OnCancel()
    {
        controller.Arrived -= OnArrived;

        // Clear the waypoint only if the entity has not yet reached it — otherwise we'd
        // be clearing an already-empty list, which is harmless but redundant.
        if (!_arrived)
        {
            controller.ClearWaypoints();
        }
    }

    /// <inheritdoc/>
    protected override void OnReset()
    {
        // Belt-and-braces: a normal Complete/Cancel cycle has already unsubscribed, but
        // if Reset is called on a mid-flight task we must not leak the subscription.
        controller.Arrived -= OnArrived;
        _arrived = false;
    }

    private void OnArrived(object? sender, EventArgs e)
    {
        _arrived = true;
    }
}
