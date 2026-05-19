namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Computes the engine's current frames-per-second by counting <see cref="Tick"/> calls and
/// dividing by elapsed time. The sample window resets once a full second has accumulated, so
/// <see cref="Fps"/> updates roughly once per second and reads as a stable integer-ish value
/// suitable for HUD display.
/// </summary>
/// <remarks>
/// Updates and renders run in lockstep in BabyBearsEngine's game loop, so a single counter
/// ticked once per frame measures both. <see cref="Fps"/> is 0 until the first sample window
/// completes.
/// </remarks>
internal sealed class FrameRateCounter
{
    // A single frame longer than this triggers a stall: we record the (very low) Fps for that
    // window and then drop the leftover elapsed time so the next window starts fresh, rather
    // than letting the stall pollute several subsequent samples.
    private const double StallThreshold = 2.0;

    private double _elapsed = 0.0;
    private int _frames = 0;

    /// <summary>The most recently sampled frames-per-second value. Zero before the first second has elapsed.</summary>
    public double Fps { get; private set; } = 0.0;

    /// <summary>
    /// Records one frame of the given duration. When the accumulated time reaches one second,
    /// <see cref="Fps"/> is recomputed and the sample window resets.
    /// </summary>
    /// <param name="elapsed">Seconds since the previous tick.</param>
    public void Tick(double elapsed)
    {
        _frames++;
        _elapsed += elapsed;

        if (_elapsed < 1.0)
        {
            return;
        }

        Fps = _frames / _elapsed;
        _frames = 0;

        if (_elapsed > StallThreshold)
        {
            Logger.Warning($"Frame took {elapsed:F2}s (>1s); FPS sample window dropped to avoid polluting subsequent samples.");
            _elapsed = 0.0;
        }
        else
        {
            // Preserve the remainder so long-run FPS stays accurate; subtracting 1.0 is exact in FP.
            _elapsed -= 1.0;
        }
    }
}
