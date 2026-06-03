using BabyBearsEngine.Worlds;

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
    /// Returns a task that removes <paramref name="thing"/> from its parent container when it
    /// completes. <paramref name="thing"/> must have a parent at completion time (the
    /// underlying <see cref="IAddable.Remove"/> call throws otherwise).
    /// </summary>
    public static ITask Remove(IAddable thing) => new Task(thing.Remove);

    /// <summary>
    /// Appends <paramref name="next"/> to the end of the chain rooted at <paramref name="first"/>
    /// and returns <paramref name="first"/> so calls can be fluently chained.
    /// </summary>
    /// <remarks>
    /// Cost is O(chain length) per call — building a chain of N tasks with fluent
    /// <c>.Then(x).Then(y)...</c> is therefore O(N²). Fine for typical short chains (a handful of
    /// steps). For very long chains, build manually by holding a tail reference and assigning
    /// <see cref="ITask.NextTask"/> directly.
    /// </remarks>
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
