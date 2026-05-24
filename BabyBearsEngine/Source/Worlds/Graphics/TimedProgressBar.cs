using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Worlds.Graphics;

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
    : ProgressBar(x, y, width, height, theme), IUpdateable
{
    private double _elapsed = 0.0;

    /// <param name="rect">Position and size relative to the parent container. The rect's width is the bar width at full fill.</param>
    /// <param name="theme">Visual styling for the bar.</param>
    /// <param name="duration">Seconds to go from empty to full.</param>
    public TimedProgressBar(Rect rect, ProgressBarTheme theme, double duration)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, duration)
    {
    }

    /// <inheritdoc/>
    public bool Active { get; set; } = true;

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
    public virtual void Update(double elapsed)
    {
        _elapsed = Math.Min(_elapsed + elapsed, duration);
        AmountFilled = (float)(_elapsed / duration);
    }
}
