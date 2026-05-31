using BabyBearsEngine.IO;
using BabyBearsEngine.Worlds;

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

    /// <summary>Configuration for the audio subsystem: SFX channel pool size, initial volumes, and music playlist defaults.</summary>
    public AudioSettings AudioSettings { get; init; } = AudioSettings.Default;

    /// <summary>Configuration for the optional debug console window (position, size, and visibility).</summary>
    public ConsoleSettings ConsoleSettings { get; init; } = ConsoleSettings.Default;

    /// <summary>Configuration for diagnostics features such as frame capture.</summary>
    public DiagnosticsSettings DiagnosticsSettings { get; init; } = DiagnosticsSettings.Default;

    /// <summary>Configuration for the game loop: target frame rate and optional timing diagnostics.</summary>
    public GameLoopSettings GameLoopSettings { get; init; } = GameLoopSettings.Default;

    /// <summary>Configuration for the IO subsystem: file-save retries and JSON serialisation options.</summary>
    public IoSettings IoSettings { get; init; } = IoSettings.Default;

    /// <summary>Configuration for the localisation subsystem: assets folder, default locale, and fallback locale.</summary>
    public LocalisationSettings LocalisationSettings { get; init; } = LocalisationSettings.Default;

    /// <summary>Configuration for the logging subsystem: sinks, severity levels, file paths, and metadata formatting.</summary>
    public LogSettings LogSettings { get; init; } = LogSettings.Default;

    /// <summary>
    /// Default MSAA sample count applied to any <see cref="Camera"/> or <see cref="Worlds.UICamera"/> that does not
    /// specify its own sample count. The GPU's actual maximum is queried at runtime and the count
    /// is clamped automatically if the requested level is not supported.
    /// Defaults to <see cref="MsaaSamples.Disabled"/>.
    /// </summary>
    public MsaaSamples DefaultCameraMsaa { get; init; } = MsaaSamples.Disabled;

    /// <summary>Configuration for the text subsystem: which atlas backend to use.</summary>
    public TextSettings TextSettings { get; init; } = TextSettings.Default;

    /// <summary>Configuration for the game window: size, title, border style, cursor, VSync, and OpenGL version.</summary>
    public WindowSettings WindowSettings { get; init; } = WindowSettings.Default;
}
