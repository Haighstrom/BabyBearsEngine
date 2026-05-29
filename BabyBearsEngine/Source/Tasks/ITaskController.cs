using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// An addable that drives an <see cref="ITask"/> chain each frame on behalf of its parent
/// (typically an <see cref="Entity"/>). Add the controller to the entity; when it runs it
/// updates <see cref="CurrentTask"/>, advances to <see cref="ITask.NextTask"/> on completion,
/// and optionally asks <see cref="GetNextTask"/> for what to do when the chain ends.
/// </summary>
public interface ITaskController : IAddable, IUpdateable
{
    /// <summary>The task currently being driven, or <c>null</c> if idle.</summary>
    ITask? CurrentTask { get; set; }

    /// <summary>
    /// Cancels the current task (firing its <see cref="ITask.TaskCancelled"/> event and
    /// running its cancellation hook) and sets <see cref="CurrentTask"/> to <c>null</c>,
    /// stopping the chain mid-flight.
    /// </summary>
    void ClearTask();

    /// <summary>
    /// Optional callback invoked when <see cref="CurrentTask"/> becomes <c>null</c> (either
    /// by chain exhaustion or <see cref="ClearTask"/>) to fetch the next task. Return
    /// <c>null</c> to stay idle.
    /// </summary>
    Func<ITask?>? GetNextTask { get; set; }
}
