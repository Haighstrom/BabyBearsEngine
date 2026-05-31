using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// Moves a <see cref="Point"/> around a circle centred at <paramref name="centre"/> with radius
/// <paramref name="radius"/>, completing one full 360° revolution per <paramref name="duration"/>
/// seconds. Equivalent to an <see cref="EllipticalMotion"/> with equal X and Y radii.
/// </summary>
public class CircularMotion(Point centre, float radius, double startAngleDegrees, double duration, bool loop = false, Func<double, double>? easing = null)
    : EllipticalMotion(centre, radius, radius, startAngleDegrees, duration, loop, easing);
