namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// Base class for all tweens. Tracks elapsed time and drives a <see cref="Progress"/> value
/// from 0 to 1 over a fixed <see cref="Duration"/>, optionally shaped by an easing function
/// (see <see cref="Easings"/>). Add to the scene graph so it receives <see cref="Update"/>
/// calls each frame.
/// <para>
/// When <see cref="Loop"/> is <see langword="false"/> (the default) the tween removes itself
/// from its parent when the cycle ends. When <see langword="true"/> it restarts automatically.
/// </para>
/// </summary>
public abstract class Tween : UpdateableBase
{
    private readonly Func<double, double>? _easing;
    private double _elapsed = 0.0;

    protected Tween(double duration, bool loop = false, Func<double, double>? easing = null)
    {
        if (duration <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        Duration = duration;
        Loop = loop;
        _easing = easing;
    }

    /// <summary>Total duration of one cycle, in seconds.</summary>
    public double Duration { get; }

    /// <summary>
    /// Raw linear progress through the current cycle, in the range [0, 1], before any easing
    /// is applied. Equivalent to <c>Elapsed / Duration</c>. Use this only when you need the
    /// unmodified time fraction; most callers want <see cref="Progress"/> instead.
    /// </summary>
    public double LinearProgress { get; private set; } = 0.0;

    /// <summary>
    /// Whether the tween loops indefinitely. When <see langword="true"/> the tween restarts
    /// from the beginning after each cycle completes. When <see langword="false"/> (default)
    /// it removes itself from its parent on completion.
    /// </summary>
    public bool Loop { get; }

    /// <summary>
    /// Eased progress through the current cycle, in the range [0, 1]. This is
    /// <see cref="LinearProgress"/> passed through the easing function supplied at construction.
    /// When no easing function was provided this equals <see cref="LinearProgress"/> exactly.
    /// This is the value to use when interpolating between start and end.
    /// </summary>
    public double Progress { get; private set; } = 0.0;

    /// <summary>Seconds remaining in the current cycle. Equivalent to <c>Duration * (1 - LinearProgress)</c>.</summary>
    public double TimeRemaining => Duration * (1.0 - LinearProgress);

    /// <summary>
    /// Fires at the end of every cycle — once for a non-looping tween (just before it removes
    /// itself), or once per iteration for a looping tween. <see cref="Progress"/> is 1 when
    /// this fires, so any derived <c>Value</c> property already reflects the final position.
    /// </summary>
    public event Action? Completed;

    /// <inheritdoc/>
    public sealed override void Update(double elapsed)
    {
        _elapsed += elapsed;

        bool cycleCompleted = _elapsed >= Duration;

        LinearProgress = Math.Clamp(_elapsed / Duration, 0.0, 1.0);
        Progress = _easing is not null ? _easing(LinearProgress) : LinearProgress;

        OnProgressUpdated();

        if (!cycleCompleted)
        {
            return;
        }

        Completed?.Invoke();

        if (Loop)
        {
            _elapsed %= Duration;
        }
        else
        {
            Remove();
        }
    }

    /// <summary>
    /// Called every update after <see cref="Progress"/> and <see cref="LinearProgress"/> have
    /// been recalculated. Override to recompute derived state such as a typed <c>Value</c>
    /// property. The base implementation does nothing.
    /// </summary>
    protected virtual void OnProgressUpdated() { }
}
