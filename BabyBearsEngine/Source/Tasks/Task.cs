using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tasks;

/// <summary>
/// Concrete <see cref="ITask"/> base. Subclasses (or callers building tasks inline) populate
/// <see cref="ActionsOnStart"/>, <see cref="CompletionConditions"/>, and
/// <see cref="ActionsOnComplete"/>; the task starts on its first <see cref="Update"/>,
/// reports <see cref="IsComplete"/> when every completion condition is true, and runs the
/// on-complete actions when <see cref="Complete"/> is called.
/// </summary>
public class Task : UpdateableBase, ITask
{
    /// <summary>A no-op task that completes immediately.</summary>
    public static ITask DoNothing => new Task();

    private bool _isCompleted = false;
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
    public virtual bool IsComplete => CompletionConditions.All(c => c());

    /// <inheritdoc/>
    public ITask? NextTask { get; set; } = null;

    /// <inheritdoc/>
    public virtual void Cancel()
    {
        if (_isCompleted)
        {
            return;
        }

        _isCompleted = true;

        OnCancel();

        TaskCancelled?.Invoke(this, EventArgs.Empty);

        _isStarted = false;
    }

    /// <inheritdoc/>
    public virtual void Complete()
    {
        _isCompleted = true;
        ActionsOnComplete.ForEach(a => a());

        TaskCompleted?.Invoke(this, EventArgs.Empty);

        _isStarted = false;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _isCompleted = false;
        _isStarted = false;
        OnReset();
    }

    /// <summary>
    /// Called by <see cref="Cancel"/> before <see cref="TaskCancelled"/> fires. Override to
    /// release resources reserved during construction or partway through the task (storage
    /// slots, equipment locks, etc.) without needing to call <c>base.Cancel()</c>.
    /// </summary>
    protected virtual void OnCancel() { }

    /// <summary>
    /// Called by <see cref="Reset"/> after clearing internal state. Override to reset
    /// subclass-specific state (timers, counters, flags) without needing to call
    /// <c>base.Reset()</c>.
    /// </summary>
    protected virtual void OnReset() { }

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
        if (_isCompleted)
        {
            return;
        }

        if (!_isStarted)
        {
            Start();
        }

        if (IsComplete)
        {
            Complete();
        }
    }

    /// <inheritdoc/>
    public event EventHandler? TaskStarted;

    /// <inheritdoc/>
    public event EventHandler? TaskCancelled;

    /// <inheritdoc/>
    public event EventHandler? TaskCompleted;
}
