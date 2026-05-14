using System.Collections.Generic;
using System.Linq;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// Concrete <see cref="ITask"/> base. Subclasses (or callers building tasks inline) populate
/// <see cref="ActionsOnStart"/>, <see cref="CompletionConditions"/>, and
/// <see cref="ActionsOnComplete"/>; the task starts on its first <see cref="Update"/>,
/// reports <see cref="IsComplete"/> when every completion condition is true, and runs the
/// on-complete actions when <see cref="Complete"/> is called.
/// </summary>
public class Task : ITask
{
    /// <summary>A no-op task that completes immediately.</summary>
    public static ITask DoNothing => new Task();

    private bool _isStarted = false;

    /// <summary>Actions invoked when <see cref="Start"/> runs.</summary>
    protected readonly List<Action> ActionsOnStart = [];

    /// <summary>Predicates checked by <see cref="IsComplete"/>; the task completes once all return true.</summary>
    protected readonly List<Func<bool>> CompletionConditions = [];

    /// <summary>Actions invoked when <see cref="Complete"/> runs.</summary>
    protected readonly List<Action> ActionsOnComplete = [];

    /// <summary>Creates an empty task. With no completion conditions it completes immediately on the next update tick.</summary>
    public Task()
    {
    }

    /// <summary>Creates a task that runs <paramref name="actionOnComplete"/> when it completes.</summary>
    public Task(Action actionOnComplete)
    {
        ActionsOnComplete.Add(actionOnComplete);
    }

    /// <inheritdoc/>
    public virtual bool Active { get; set; } = true;

    /// <inheritdoc/>
    public virtual bool IsComplete => CompletionConditions.All(c => c());

    /// <inheritdoc/>
    public ITask? NextTask { get; set; } = null;

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
    public virtual void Update(double elapsed)
    {
        if (!_isStarted)
        {
            Start();
            _isStarted = true;
        }
    }

    /// <inheritdoc/>
    public event EventHandler? TaskStarted;

    /// <inheritdoc/>
    public event EventHandler? TaskCompleted;
}
