namespace BabyBearsEngine;

/// <summary>
/// Configuration for the game loop: target frame rate and optional console timing diagnostics.
/// </summary>
public sealed record GameLoopSettings()
{
    /// <summary>The default game loop settings: 60 fps, no console timing output.</summary>
    public static GameLoopSettings Default => new();

    /// <summary>
    /// Target number of update and render cycles per second. Defaults to 60.
    /// </summary>
    public int TargetFramesPerSecond { get; set; } = 60;

    /// <summary>
    /// When true, frame-timing statistics are periodically written to the console. Defaults to false.
    /// </summary>
    public bool PeriodicallyLogGameLoopInfoInConsole { get; set; } = false;
}
