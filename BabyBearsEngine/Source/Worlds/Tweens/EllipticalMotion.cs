using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// Moves a <see cref="Point"/> around an ellipse centred at <paramref name="centre"/> with
/// horizontal radius <paramref name="radiusX"/> and vertical radius <paramref name="radiusY"/>,
/// completing one full 360° revolution per <paramref name="duration"/> seconds. The motion
/// begins at <paramref name="startAngleDegrees"/>, measured counter-clockwise from the positive
/// X axis. With <c>loop: true</c> the orbit continues indefinitely.
/// </summary>
public class EllipticalMotion(Point centre, float radiusX, float radiusY, double startAngleDegrees, double duration, bool loop = false, Func<double, double>? easing = null)
    : Tween(duration, loop, easing)
{
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double FullRevolutionRadians = 2.0 * Math.PI;

    private readonly double _startAngleRadians = startAngleDegrees * DegreesToRadians;

    /// <summary>
    /// The current angle around the ellipse, in degrees, measured counter-clockwise from the
    /// positive X axis. Equals <c>startAngleDegrees</c> at <see cref="Tween.Progress"/> 0 and
    /// <c>startAngleDegrees + 360</c> at <see cref="Tween.Progress"/> 1.
    /// </summary>
    public double AngleDegrees => startAngleDegrees + 360.0 * Progress;

    /// <summary>The current position on the ellipse.</summary>
    public Point Value { get; private set; } = new(
        centre.X + (float)Math.Cos(startAngleDegrees * DegreesToRadians) * radiusX,
        centre.Y + (float)Math.Sin(startAngleDegrees * DegreesToRadians) * radiusY);

    protected override void OnProgressUpdated()
    {
        double angleRadians = _startAngleRadians + FullRevolutionRadians * Progress;
        Value = new Point(
            centre.X + (float)Math.Cos(angleRadians) * radiusX,
            centre.Y + (float)Math.Sin(angleRadians) * radiusY);
    }
}
