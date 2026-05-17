namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// A timer that fires <see cref="Tween.Completed"/> after a set <paramref name="duration"/>,
/// then either removes itself from its parent (one-shot) or restarts (looping).
/// <para>
/// As a <see cref="Tween"/>, <see cref="Tween.Progress"/> reflects how far through the
/// current interval the alarm is (0 at the start, 1 when it fires), optionally shaped by
/// an <paramref name="easing"/> function — useful for driving animations or progress bars
/// off the same timer that governs the event.
/// </para>
/// </summary>
public class Alarm(double duration, bool loop = false, Func<double, double>? easing = null)
    : Tween(duration, loop, easing)
{
    /// <summary>
    /// Creates an <see cref="Alarm"/> and immediately subscribes <paramref name="onCompleted"/>
    /// to <see cref="Tween.Completed"/>, as a convenience alternative to subscribing after
    /// construction.
    /// </summary>
    public Alarm(double duration, Action onCompleted, bool loop = false, Func<double, double>? easing = null)
        : this(duration, loop, easing)
    {
        Completed += onCompleted;
    }
}
