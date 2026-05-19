namespace BabyBearsEngine;

/// <summary>
/// Read-only snapshot of engine-level runtime information. Currently surfaces frame rate;
/// expected to grow over time as more diagnostics become useful to game code.
/// </summary>
public interface IEngineInfo
{
    /// <summary>
    /// The engine's current frames-per-second, sampled approximately once per second. Reads
    /// 0 until the first sample window completes after startup. Updates and renders run in
    /// lockstep, so this value reflects both rates.
    /// </summary>
    double Fps { get; }
}
