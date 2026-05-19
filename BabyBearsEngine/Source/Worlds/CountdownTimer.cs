namespace BabyBearsEngine.Worlds;

/// <summary>
/// A countdown timer that fires <see cref="Ticked"/> at regular intervals and
/// <see cref="Completed"/> when the full duration elapses, then removes itself.
/// Add to the scene graph so it receives <see cref="Update"/> calls each frame.
/// </summary>
public class CountdownTimer : UpdateableBase
{
    private double _nextTickThreshold;

    /// <param name="duration">Total time in seconds before <see cref="Completed"/> fires. Must be positive.</param>
    /// <param name="tickInterval">How often <see cref="Ticked"/> fires, in seconds. Must be positive. Defaults to 1.</param>
    /// <param name="onCompleted">If provided, subscribed to <see cref="Completed"/> immediately.</param>
    /// <param name="onTicked">If provided, subscribed to <see cref="Ticked"/> immediately.</param>
    public CountdownTimer(double duration, double tickInterval = 1.0, Action? onCompleted = null, Action? onTicked = null)
    {
        if (duration <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        if (tickInterval <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickInterval), "TickInterval must be positive.");
        }

        Duration = duration;
        TickInterval = tickInterval;
        _nextTickThreshold = tickInterval;

        if (onCompleted is not null)
        {
            Completed += onCompleted;
        }

        if (onTicked is not null)
        {
            Ticked += onTicked;
        }
    }

    /// <summary>Seconds elapsed since the timer started.</summary>
    public double Elapsed { get; private set; } = 0.0;

    /// <summary>Total duration in seconds.</summary>
    public double Duration { get; }

    /// <summary>How often <see cref="Ticked"/> fires, in seconds.</summary>
    public double TickInterval { get; }

    /// <summary>Seconds remaining until <see cref="Completed"/> fires. Clamped to 0 on overshoot.</summary>
    public double TimeRemaining => Math.Max(0.0, Duration - Elapsed);

    /// <summary>Fires when <see cref="Completed"/> fires.</summary>
    public event Action? Completed;

    /// <summary>
    /// Fires each time a <see cref="TickInterval"/> boundary is crossed, strictly before
    /// <see cref="Duration"/> is reached. Multiple ticks may fire in a single
    /// <see cref="Update"/> call if a large frame spans several boundaries.
    /// </summary>
    public event Action? Ticked;

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        if (!Active)
        {
            return;
        }

        Elapsed += elapsed;

        while (Elapsed >= _nextTickThreshold && _nextTickThreshold < Duration)
        {
            Ticked?.Invoke();
            _nextTickThreshold += TickInterval;
        }

        if (Elapsed >= Duration)
        {
            Completed?.Invoke();
            Remove();
        }
    }
}
