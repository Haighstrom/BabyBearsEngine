using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// Default <see cref="ITaskController"/> implementation. Add to an entity to drive an
/// <see cref="ITask"/> chain each frame.
/// </summary>
/// <remarks>
/// On each <see cref="Update"/>: if <see cref="CurrentTask"/> is active, update it; if it
/// reports complete, call its <see cref="ITask.Complete"/>, advance to its
/// <see cref="ITask.NextTask"/>, and (when the chain ends) ask <see cref="GetNextTask"/>
/// for what to do next. Updates are skipped when the controller has no parent — this means
/// removing the controller (or its entity) from the world cleanly stops the chain.
/// </remarks>
public class TaskController : AddableBase, ITaskController
{
    /// <summary>Creates a controller with no current task and no <see cref="GetNextTask"/> callback.</summary>
    public TaskController()
    {
        CurrentTask = null;
        GetNextTask = null;
    }

    /// <summary>Creates a controller pre-loaded with a sequence of tasks chained via <see cref="ITask.NextTask"/>.</summary>
    public TaskController(ITask[] tasks)
    {
        Ensure.ArgumentCollectionNotNullOrEmpty(tasks, nameof(tasks));

        for (int i = 0; i < tasks.Length - 1; i++)
        {
            tasks[i].NextTask = tasks[i + 1];
        }

        CurrentTask = tasks[0];
    }

    /// <summary>Creates an idle controller that pulls its next task from <paramref name="getNextTask"/> each time the chain ends.</summary>
    public TaskController(Func<ITask?>? getNextTask = null)
        : this(null, getNextTask)
    {
    }

    /// <summary>Creates a controller starting at <paramref name="firstTask"/>, falling back to <paramref name="getNextTask"/> when the chain ends.</summary>
    public TaskController(ITask? firstTask, Func<ITask?>? getNextTask = null)
    {
        CurrentTask = firstTask;
        GetNextTask = getNextTask;
    }

    /// <inheritdoc/>
    public virtual bool Active { get; set; } = true;

    /// <inheritdoc/>
    public ITask? CurrentTask { get; set; }

    /// <inheritdoc/>
    public Func<ITask?>? GetNextTask { get; set; }

    /// <inheritdoc/>
    public void ClearTask() => CurrentTask = null;

    /// <inheritdoc/>
    public virtual void Update(double elapsed)
    {
        if (Parent is null || (Parent is IAddable addable && addable.Parent is null))
        {
            return;
        }

        if (CurrentTask is not null && CurrentTask.Active)
        {
            CurrentTask.Update(elapsed);

            if (CurrentTask is not null && CurrentTask.IsComplete)
            {
                CurrentTask = CurrentTask.NextTask;
            }
        }

        if (CurrentTask is null && GetNextTask is not null)
        {
            CurrentTask = GetNextTask();
        }
    }
}
