using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// A single unit of work driven by a <see cref="ITaskController"/> (or chained into a
/// <see cref="TaskGroup"/>). Tasks are updated each frame and complete when their internal
/// conditions are met, optionally chaining into a <see cref="NextTask"/>.
/// </summary>
public interface ITask : IUpdateable
{
    /// <summary>True when the task's completion conditions are satisfied.</summary>
    bool IsComplete { get; }

    /// <summary>The task to run after this one completes, or <c>null</c> to stop the chain.</summary>
    ITask? NextTask { get; set; }

    /// <summary>
    /// Cancels the task without completing it: ends the task in a terminal state, runs the
    /// cancellation hook, and fires <see cref="TaskCancelled"/>. <see cref="TaskCompleted"/>
    /// is <em>not</em> fired and on-complete actions are <em>not</em> run. Idempotent — a
    /// no-op if the task has already completed or been cancelled.
    /// </summary>
    /// <remarks>
    /// Called automatically by <see cref="ITaskController.ClearTask"/> and when a controller
    /// detects its parent has been removed from the world mid-flight, so subclasses can rely
    /// on it firing whenever the task is stopped early.
    /// </remarks>
    void Cancel();

    /// <summary>Runs the on-complete actions and fires <see cref="TaskCompleted"/>.</summary>
    void Complete();

    /// <summary>Resets internal state so the task can run again from the beginning.</summary>
    void Reset();

    /// <summary>
    /// Manually trigger start. Not usually required — adding an <see cref="ITask"/> to a
    /// <see cref="ITaskController"/>, <see cref="TaskGroup"/>, or as another task's
    /// <see cref="NextTask"/> will call <see cref="Start"/> automatically on the first
    /// <see cref="IUpdateable.Update"/>.
    /// </summary>
    void Start();

    /// <summary>Raised when the task starts (on its first update or via explicit <see cref="Start"/>).</summary>
    event EventHandler? TaskStarted;

    /// <summary>Raised when <see cref="Cancel"/> ends the task without completion.</summary>
    event EventHandler? TaskCancelled;

    /// <summary>Raised when the task completes.</summary>
    event EventHandler? TaskCompleted;
}
