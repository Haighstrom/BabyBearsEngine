using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="ProgressBar"/> that fills itself automatically over a fixed duration.
/// Each update tick advances the fill; <see cref="ProgressBar.BarFilled"/> fires when it reaches 1.
/// </summary>
/// <param name="x">X position relative to the parent container.</param>
/// <param name="y">Y position relative to the parent container.</param>
/// <param name="width">Width in pixels at full fill.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="theme">Visual styling for the bar.</param>
/// <param name="duration">Seconds to go from empty to full.</param>
public class TimedProgressBar(float x, float y, float width, float height, ProgressBarTheme theme, double duration) 
    : ProgressBar(x, y, width, height, theme)
{
    private double _elapsed = 0.0;

    /// <summary>
    /// Restarts the timer from empty, optionally with a new duration.
    /// </summary>
    public void Restart(double? newDuration = null)
    {
        if (newDuration.HasValue)
        {
            duration = newDuration.Value;
        }

        _elapsed = 0.0;
        AmountFilled = 0f;
    }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        _elapsed = Math.Min(_elapsed + elapsed, duration);
        AmountFilled = (float)(_elapsed / duration);

        base.Update(elapsed);
    }
}
