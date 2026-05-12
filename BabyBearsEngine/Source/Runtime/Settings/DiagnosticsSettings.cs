using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine;

/// <summary>
/// Settings for diagnostic and testing features. None of these should be enabled in production —
/// they trade runtime cost for observability.
/// </summary>
public sealed record DiagnosticsSettings()
{
    public static DiagnosticsSettings Default => new();

    /// <summary>
    /// When true, the engine captures the full back buffer after every <see cref="Worlds.IWorld.Draw"/>
    /// and makes it available via <see cref="IScreenCapture"/>. Stalls the GPU pipeline once per frame —
    /// intended for tests, not for shipped builds. Defaults to false.
    /// </summary>
    public bool CaptureFrames { get; init; } = false;

    /// <summary>
    /// Emits a log warning for each diagnostic flag enabled in a Release build of the engine. Intended
    /// to be called once at startup so a misconfigured production build surfaces the mistake visibly,
    /// without blocking deliberate uses (benchmarks, debug menus that ship enabled, etc.). No-op in
    /// Debug builds.
    /// </summary>
    public void WarnIfEnabledInRelease()
    {
#if !DEBUG
        if (CaptureFrames)
        {
            Logger.Log("WARNING: DiagnosticsSettings.CaptureFrames is enabled in a Release build. This significantly increases per-frame cost and should not be shipped to end users.");
        }
#endif
    }
}
