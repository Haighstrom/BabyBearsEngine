namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Concrete <see cref="IEngineInfo"/> registered by the engine at startup. Aggregates the
/// individual diagnostic counters (currently just <see cref="FrameRateCounter"/>) and exposes
/// them through the public interface. The engine drives the underlying state by calling
/// <see cref="Tick"/> once per frame.
/// </summary>
internal sealed class EngineInfo : IEngineInfo
{
    private readonly FrameRateCounter _frameRate = new();

    /// <inheritdoc/>
    public double Fps => _frameRate.Fps;

    /// <summary>Records one frame of the given duration. Called by the engine once per render frame.</summary>
    /// <param name="elapsed">Seconds since the previous tick.</param>
    public void Tick(double elapsed) => _frameRate.Tick(elapsed);
}
