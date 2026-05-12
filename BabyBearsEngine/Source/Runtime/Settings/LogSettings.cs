namespace BabyBearsEngine;

public record class LogSettings()
{
    public static LogSettings Default => new();

    /// <summary>
    /// A preset that silences all log output. Useful for tests that don't want console noise or
    /// log files cluttering the working directory. Equivalent to setting all sink masks to
    /// <see cref="LogLevel.None"/> and nulling both file paths.
    /// </summary>
    public static LogSettings Silent => new()
    {
        ConsoleLevels = LogLevel.None,
        FileLevels = LogLevel.None,
        ErrorFileLevels = LogLevel.None,
        FilePath = null,
        ErrorFilePath = null,
    };

    /// <summary>
    /// Mask of severities written to the console. Set to <see cref="LogLevel.None"/> to disable
    /// console output (e.g. for headless services). Defaults to <see cref="LogLevel.All"/>.
    /// </summary>
    public LogLevel ConsoleLevels { get; init; } = LogLevel.All;

    /// <summary>
    /// When true, calls to <see cref="Diagnostics.Logger.GLError"/> are deduplicated by source
    /// location for the lifetime of the process — repeat calls from the same <c>(file, line)</c>
    /// are silently dropped. Prevents a persistent GL failure inside a render loop from filling
    /// the log with one entry per frame. Defaults to true. Call <see cref="Diagnostics.Logger.ResetDedupe"/>
    /// to clear the seen set; <c>Logger.Initialise</c> also clears it.
    /// </summary>
    public bool DedupeGLErrors { get; init; } = true;

    /// <summary>
    /// Path for a separate file capturing the levels in <see cref="ErrorFileLevels"/>. Always
    /// appended (never wiped) so historical errors survive across runs and can be attached to bug
    /// reports. Lazy-created — the file isn't touched on disk until the first matching message
    /// fires. Set to null to disable. Defaults to "errors.log".
    /// </summary>
    public string? ErrorFilePath { get; init; } = "errors.log";

    /// <summary>
    /// Mask of severities written to <see cref="ErrorFilePath"/>. Defaults to
    /// <see cref="LogLevel.ErrorAndAbove"/>. Ignored when <see cref="ErrorFilePath"/> is null.
    /// </summary>
    public LogLevel ErrorFileLevels { get; init; } = LogLevel.ErrorAndAbove;

    /// <summary>
    /// Controls how <see cref="FilePath"/> is handled at startup — append, overwrite, or create
    /// a new timestamped file per run. Defaults to <see cref="LogFileMode.OverwriteExisting"/>.
    /// Ignored when <see cref="FilePath"/> is null. Note this never applies to <see cref="ErrorFilePath"/>,
    /// which always appends.
    /// </summary>
    public LogFileMode FileMode { get; init; } = LogFileMode.OverwriteExisting;

    /// <summary>
    /// Mask of severities written to <see cref="FilePath"/>. Defaults to
    /// <see cref="LogLevel.AllButErrors"/> on the assumption that errors will be captured by the
    /// dedicated <see cref="ErrorFilePath"/>; set to <see cref="LogLevel.All"/> if you want errors
    /// duplicated in the main log. Ignored when <see cref="FilePath"/> is null.
    /// </summary>
    public LogLevel FileLevels { get; init; } = LogLevel.AllButErrors;

    /// <summary>
    /// Path to write the main log file to. Set to null to disable file logging.
    /// Defaults to "log.log" in the current working directory.
    /// </summary>
    public string? FilePath { get; init; } = "log.log";

    /// <summary>
    /// Which metadata fields are prefixed onto each log message. Applies to all sinks.
    /// Defaults to <see cref="LogMetadata.Default"/> (level + timestamp). Add
    /// <see cref="LogMetadata.CallerInfo"/> for a <c>[FileName.cs:Line MemberName]</c> prefix.
    /// </summary>
    public LogMetadata MessageMetadata { get; init; } = LogMetadata.Default;
}
