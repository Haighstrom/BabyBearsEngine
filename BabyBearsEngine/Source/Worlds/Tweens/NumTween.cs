namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// Interpolates a <see cref="double"/> value from <paramref name="startValue"/> to
/// <paramref name="endValue"/> over <paramref name="duration"/> seconds.
/// <see cref="Value"/> is updated each frame and reflects the current position.
/// </summary>
public class NumTween(double startValue, double endValue, double duration, bool loop = false, Func<double, double>? easing = null) : Tween(duration, loop, easing)
{
    private readonly double _startValue = startValue;
    private readonly double _range = endValue - startValue;

    /// <summary>The current interpolated value, between <c>startValue</c> and <c>endValue</c>.</summary>
    public double Value { get; private set; } = startValue;

    protected override void OnProgressUpdated()
    {
        Value = _startValue + _range * Progress;
    }
}
