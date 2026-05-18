namespace BabyBearsEngine.Worlds.Tweens;

/// <summary>
/// Interpolates a <see cref="Colour"/> from <paramref name="from"/> to <paramref name="to"/>
/// over <paramref name="duration"/> seconds. <see cref="Value"/> is updated each frame via
/// <see cref="Colour.Lerp"/>, which clamps channels to [0, 255] so easing functions that
/// briefly overshoot (e.g. <see cref="Easings.EaseOutBack"/>) will not produce invalid colours.
/// </summary>
public class ColourTween(Colour from, Colour to, double duration, bool loop = false, Func<double, double>? easing = null) : Tween(duration, loop, easing)
{

    /// <summary>The current interpolated colour.</summary>
    public Colour Value { get; private set; } = from;

    protected override void OnProgressUpdated()
    {
        Value = Colour.Lerp(from, to, Progress);
    }
}
