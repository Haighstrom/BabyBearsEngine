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

    /// <summary>Raised when the task completes.</summary>
    event EventHandler? TaskCompleted;
}
