namespace BabyBearsEngine;

/// <summary>
/// Top-level configuration bag passed to <see cref="GameLauncher.Run"/>. Each nested settings
/// record controls an independent subsystem; all properties default to their respective
/// <c>Default</c> presets so only the values you want to change need to be specified.
/// </summary>
public record class ApplicationSettings()
{
    /// <summary>The default application settings — all subsystems at their defaults.</summary>
    public static ApplicationSettings Default => new();

    /// <summary>Configuration for the optional debug console window (position, size, and visibility).</summary>
    public ConsoleSettings ConsoleSettings { get; init; } = ConsoleSettings.Default;

    /// <summary>Configuration for diagnostics features such as frame capture.</summary>
    public DiagnosticsSettings DiagnosticsSettings { get; init; } = DiagnosticsSettings.Default;

    /// <summary>Configuration for the game loop: target frame rate and optional timing diagnostics.</summary>
    public GameLoopSettings GameLoopSettings { get; init; } = GameLoopSettings.Default;

    /// <summary>Configuration for the IO subsystem: file-save retries and JSON serialisation options.</summary>
    public IoSettings IoSettings { get; init; } = IoSettings.Default;

    /// <summary>Configuration for the logging subsystem: sinks, severity levels, file paths, and metadata formatting.</summary>
    public LogSettings LogSettings { get; init; } = LogSettings.Default;

    /// <summary>Configuration for the game window: size, title, border style, cursor, VSync, and OpenGL version.</summary>
    public WindowSettings WindowSettings { get; init; } = WindowSettings.Default;
}
