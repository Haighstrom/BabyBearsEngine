using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// A task that wraps a chain of subtasks: starts the first, advances to <see cref="ITask.NextTask"/>
/// as each completes, and reports <see cref="IsComplete"/> once the chain is exhausted <em>and</em>
/// the group's own <see cref="CompletionConditions"/> are satisfied.
/// </summary>
public class TaskGroup : UpdateableBase, ITask
{
    private bool _isStarted = false;

    /// <summary>Actions invoked when the group itself starts.</summary>
    protected readonly List<Action> ActionsOnStart = [];

    /// <summary>Predicates checked alongside chain-exhaustion in <see cref="IsComplete"/>.</summary>
    protected readonly List<Func<bool>> CompletionConditions = [];

    /// <summary>Actions invoked when <see cref="Complete"/> runs.</summary>
    protected readonly List<Action> ActionsOnComplete = [];

    /// <summary>Creates a group from a sequence of tasks, chaining them via <see cref="ITask.NextTask"/>.</summary>
    public TaskGroup(params ITask[] tasks)
    {
        if (tasks.Length == 0)
        {
            return;
        }

        for (int i = 0; i < tasks.Length - 1; i++)
        {
            tasks[i].NextTask = tasks[i + 1];
        }

        CurrentTask = tasks[0];
    }

    /// <summary>Creates a group starting at <paramref name="firstTask"/> (or empty if null).</summary>
    public TaskGroup(ITask? firstTask = null)
    {
        CurrentTask = firstTask;
    }

    /// <summary>The currently-running task in the chain, or <c>null</c> if the chain is exhausted.</summary>
    public ITask? CurrentTask { get; set; }

    /// <summary>Walks the <see cref="ITask.NextTask"/> chain from <see cref="CurrentTask"/> and returns the tail.</summary>
    public ITask? LastTask
    {
        get
        {
            if (CurrentTask is null)
            {
                return null;
            }

            var task = CurrentTask;
            while (task.NextTask is not null)
            {
                task = task.NextTask;
            }
            return task;
        }
    }

    /// <inheritdoc/>
    public bool IsComplete => CurrentTask is null && CompletionConditions.All(c => c());

    /// <inheritdoc/>
    public ITask? NextTask { get; set; } = null;

    /// <summary>Appends tasks to the end of the chain, preserving any existing chain.</summary>
    public void AddTasks(params ITask[] tasks) => AddTasks((IList<ITask>)tasks);

    /// <summary>Appends tasks to the end of the chain, preserving any existing chain.</summary>
    public void AddTasks(IList<ITask> tasks)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        for (int i = 0; i < tasks.Count - 1; i++)
        {
            tasks[i].NextTask = tasks[i + 1];
        }

        if (LastTask is null)
        {
            CurrentTask = tasks[0];
        }
        else
        {
            LastTask.NextTask = tasks[0];
        }
    }

    /// <inheritdoc/>
    public virtual void Complete()
    {
        ActionsOnComplete.ForEach(a => a());

        TaskCompleted?.Invoke(this, EventArgs.Empty);

        _isStarted = false;
    }

    /// <inheritdoc/>
    public virtual void Reset()
    {
        _isStarted = false;
    }

    /// <inheritdoc/>
    public virtual void Start()
    {
        ActionsOnStart.ForEach(a => a());

        TaskStarted?.Invoke(this, EventArgs.Empty);

        _isStarted = true;
    }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        if (!_isStarted)
        {
            Start();
            _isStarted = true;
        }

        if (CurrentTask is not null && CurrentTask.Active)
        {
            CurrentTask.Update(elapsed);

            if (CurrentTask.IsComplete)
            {
                CurrentTask.Complete();
                CurrentTask = CurrentTask.NextTask;
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler? TaskStarted;

    /// <inheritdoc/>
    public event EventHandler? TaskCompleted;
}
