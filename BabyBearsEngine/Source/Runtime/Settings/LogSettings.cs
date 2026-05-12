using Microsoft.Extensions.Logging;

namespace BabyBearsEngine;

public record class LogSettings()
{
    public static LogSettings Default => new();

    /// <summary>
    /// Path to write the log file to. Set to null to disable file logging.
    /// Defaults to "log.txt" in the current working directory.
    /// </summary>
    public string? FilePath { get; init; } = "log.txt";

    /// <summary>
    /// When true, every log line is prefixed with the source location of the caller, in the form
    /// <c>[FileName.cs:Line MemberName]</c>. Useful for debugging; adds visual noise.
    /// Defaults to false.
    /// </summary>
    public bool IncludeCallerInfo { get; init; } = false;

    /// <summary>
    /// The minimum severity that gets emitted. Anything below this level is filtered out.
    /// Set to <see cref="LogLevel.None"/> to disable all logging (useful in tests).
    /// Defaults to <see cref="LogLevel.Information"/>.
    /// </summary>
    public LogLevel MinimumLevel { get; init; } = LogLevel.Information;
}
