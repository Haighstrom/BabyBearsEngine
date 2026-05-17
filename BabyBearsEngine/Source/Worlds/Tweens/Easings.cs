namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// A library of common easing functions for use with <see cref="Tween"/> subclasses.
/// Each function maps a normalised input <c>t</c> in [0, 1] to a shaped output, with
/// <c>f(0) == 0</c> and <c>f(1) == 1</c> guaranteed for all entries.
/// <para>
/// Note: <see cref="EaseInBack"/>, <see cref="EaseOutBack"/> intentionally overshoot
/// (output briefly outside [0, 1]) to produce a springy feel. <see cref="Colour.Lerp"/>
/// clamps its input so colour tweens are safe with any easing function.
/// </para>
/// <para>
/// Pass any of these as the <c>easing</c> parameter to a tween constructor:
/// <code>new NumTween(0, 100, duration: 2.0, easing: Easings.EaseOutQuad)</code>
/// </para>
/// </summary>
public static class Easings
{
    /// <summary>No easing — linear progression. Equivalent to passing no easing function.</summary>
    public static Func<double, double> Linear { get; } = t => t;

    /// <summary>Quadratic ease-in: slow start, fast finish.</summary>
    public static Func<double, double> EaseInQuad { get; } = t => t * t;

    /// <summary>Quadratic ease-out: fast start, slow finish.</summary>
    public static Func<double, double> EaseOutQuad { get; } = t => t * (2 - t);

    /// <summary>Quadratic ease-in-out: slow start and finish, fast middle.</summary>
    public static Func<double, double> EaseInOutQuad { get; } =
        t => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

    /// <summary>Cubic ease-in: slow start, fast finish.</summary>
    public static Func<double, double> EaseInCubic { get; } = t => t * t * t;

    /// <summary>Cubic ease-out: fast start, slow finish.</summary>
    public static Func<double, double> EaseOutCubic { get; } = t => 1 + (t - 1) * (t - 1) * (t - 1);

    /// <summary>Cubic ease-in-out: slow start and finish, fast middle.</summary>
    public static Func<double, double> EaseInOutCubic { get; } =
        t => t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    /// <summary>Sine ease-in: gentle slow start.</summary>
    public static Func<double, double> EaseInSine { get; } = t => 1 - Math.Cos(t * Math.PI / 2);

    /// <summary>Sine ease-out: gentle slow finish.</summary>
    public static Func<double, double> EaseOutSine { get; } = t => Math.Sin(t * Math.PI / 2);

    /// <summary>Sine ease-in-out: gentle slow start and finish.</summary>
    public static Func<double, double> EaseInOutSine { get; } = t => -(Math.Cos(Math.PI * t) - 1) / 2;

    /// <summary>
    /// Back ease-in: slight pullback behind the start before moving forward.
    /// Output briefly dips below 0 before rising to 1.
    /// </summary>
    public static Func<double, double> EaseInBack { get; } = t =>
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return c3 * t * t * t - c1 * t * t;
    };

    /// <summary>
    /// Back ease-out: overshoots the end before settling back.
    /// Output briefly exceeds 1 before returning to 1.
    /// </summary>
    public static Func<double, double> EaseOutBack { get; } = t =>
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    };

    /// <summary>Bouncing ease-out: the value overshoots and bounces back like a ball landing.</summary>
    public static Func<double, double> EaseOutBounce { get; } = t =>
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;

        if (t < 1 / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2 / d1)
        {
            double u = t - 1.5 / d1;
            return n1 * u * u + 0.75;
        }
        else if (t < 2.5 / d1)
        {
            double u = t - 2.25 / d1;
            return n1 * u * u + 0.9375;
        }
        else
        {
            double u = t - 2.625 / d1;
            return n1 * u * u + 0.984375;
        }
    };
}
