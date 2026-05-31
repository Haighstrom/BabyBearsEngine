using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// Interpolates a <see cref="Point"/> in a straight line from <paramref name="start"/> to
/// <paramref name="end"/> over <paramref name="duration"/> seconds. <see cref="Value"/> is
/// updated each frame and reflects the current position.
/// </summary>
public class LinearMotion(Point start, Point end, double duration, bool loop = false, Func<double, double>? easing = null)
    : Tween(duration, loop, easing)
{
    private readonly Point _start = start;
    private readonly Point _range = new(end.X - start.X, end.Y - start.Y);

    /// <summary>The current interpolated position.</summary>
    public Point Value { get; private set; } = start;

    protected override void OnProgressUpdated()
    {
        Value = new Point(
            _start.X + _range.X * (float)Progress,
            _start.Y + _range.Y * (float)Progress);
    }
}
