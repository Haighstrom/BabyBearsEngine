using BabyBearsEngine.Geometry;
using BabyBearsEngine.Pathfinding;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// Pathfinds from <paramref name="start"/> to <paramref name="goal"/> via
/// <paramref name="controller"/> and walks the entity along the result. Completes when the
/// entity arrives; if no path exists the task cancels itself immediately (firing
/// <see cref="ITask.TaskCancelled"/>) so a chain can branch on path failure.
/// </summary>
/// <param name="controller">Pathing controller for the entity being moved.</param>
/// <param name="start">Start node for the search.</param>
/// <param name="goal">Goal node for the search.</param>
/// <typeparam name="TNode">Pathfinder node type.</typeparam>
public class PathfindToTask<TNode>(IPathingController<TNode> controller, TNode start, TNode goal) : Task
    where TNode : IPathfindNode<TNode>, IPosition
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
        base.Start();

        if (!controller.TryFindPath(start, goal))
        {
            Cancel();
        }
    }

    /// <inheritdoc/>
    protected override void OnCancel()
    {
        controller.Arrived -= OnArrived;

        if (!_arrived)
        {
            controller.ClearPath();
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
