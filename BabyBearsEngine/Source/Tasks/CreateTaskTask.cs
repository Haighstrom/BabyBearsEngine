namespace BabyBearsEngine.Tasks;

/// <summary>
/// Dynamically inserts a freshly-created task into the chain at runtime: on <see cref="Start"/>
/// it builds a new task via <paramref name="taskCreator"/>, makes it this task's new
/// <see cref="ITask.NextTask"/>, and re-attaches the originally-queued next task behind the
/// new one. Turns a static chain <c>[A → C]</c> into <c>[A → B → C]</c> the moment A starts.
/// </summary>
/// <remarks>
/// This is the mechanism that lets prebuilt sequential chains support branching without a
/// parallel/conditional task primitive — the deferred <see cref="Func{ITask}"/> can inspect
/// world state at the point of insertion and pick which subtask to splice in. See issue #64
/// for the design discussion of why the <see cref="ITask.NextTask"/> model is sufficient.
/// </remarks>
/// <param name="taskCreator">Builds the task to splice in. Invoked once, on <see cref="Start"/>.</param>
public class CreateTaskTask(Func<ITask> taskCreator) : Task
{
    /// <inheritdoc/>
    public override void Start()
    {
        base.Start();

        ITask? existingNextTask = NextTask;

        NextTask = taskCreator();

        if (existingNextTask is not null)
        {
            NextTask.NextTask = existingNextTask;
        }
    }
}
