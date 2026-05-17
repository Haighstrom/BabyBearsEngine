namespace BabyBearsEngine.Tasks;

/// <summary>
/// Factory helpers for building <see cref="ITask"/> instances inline, without named subclasses.
/// </summary>
/// <remarks>
/// Use these for throwaway, one-off tasks. Named subclasses still pay off for reused or
/// domain-specific behaviour.
/// <para>
/// Chain tasks with <see cref="Then"/>:
/// <code>
/// Tasks.Run(OpenDoor).Then(Tasks.Delay(1)).Then(Tasks.Run(WalkIn))
/// </code>
/// </para>
/// </remarks>
public static class Tasks
{
    /// <summary>
    /// Returns a task that immediately completes and invokes <paramref name="action"/> on completion.
    /// </summary>
    public static ITask Run(Action action) => new Task(action);

    /// <summary>
    /// Returns a task that completes once <paramref name="condition"/> returns <c>true</c>.
    /// </summary>
    public static ITask WaitFor(Func<bool> condition) => new ConditionTask(condition);

    /// <summary>
    /// Returns a task that completes after <paramref name="seconds"/> have elapsed.
    /// </summary>
    public static ITask Delay(double seconds) => new WaitTask(seconds);

    /// <summary>
    /// Appends <paramref name="next"/> to the end of the chain rooted at <paramref name="first"/>
    /// and returns <paramref name="first"/> so calls can be fluently chained.
    /// </summary>
    public static ITask Then(this ITask first, ITask next)
    {
        ITask current = first;

        while (current.NextTask is not null)
        {
            current = current.NextTask;
        }

        current.NextTask = next;
        return first;
    }

    private sealed class ConditionTask(Func<bool> condition) : Task
    {
        public override bool IsComplete => condition();
    }
}
