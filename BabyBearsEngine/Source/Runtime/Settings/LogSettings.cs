namespace BabyBearsEngine;

/// <summary>
/// Configuration for the logging subsystem: which severities flow to the console, the main
/// log file, and the dedicated error file; file-handling modes; metadata formatting; and
/// GL-error deduplication.
/// </summary>
public record class LogSettings()
{
    /// <summary>The default log settings: all levels to console, "log.log" as the main file, "errors.log" for errors.</summary>
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
        ErrorArchivePath = null,
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
    /// Path for the current-run error file. Wiped at startup — the previous run's content is
    /// moved to <see cref="ErrorArchivePath"/> before the wipe. Lazy-created: the file is not
    /// touched until the first matching message fires, so a clean run leaves no file on disk.
    /// Set to null to disable. Defaults to "errors.log".
    /// </summary>
    public string? ErrorFilePath { get; init; } = "errors.log";

    /// <summary>
    /// Path for the error archive. At startup the previous run's errors are prepended here
    /// (newest run first) and the archive is trimmed to <see cref="ErrorArchiveMaxRuns"/> runs.
    /// Set to null to disable archiving. Defaults to "error_archive.log".
    /// </summary>
    public string? ErrorArchivePath { get; init; } = "error_archive.log";

    /// <summary>
    /// Maximum number of past runs retained in <see cref="ErrorArchivePath"/>. Defaults to 50.
    /// Ignored when <see cref="ErrorArchivePath"/> is null.
    /// </summary>
    public int ErrorArchiveMaxRuns { get; init; } = 50;

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
    /// Maximum number of per-run log files retained when <see cref="FileMode"/> is
    /// <see cref="LogFileMode.NewFilePerRun"/>. At startup, older matching files are deleted
    /// (oldest first by filename timestamp) so the total count after the new run's file is
    /// written sits at this limit. Defaults to 50. Set to 0 or negative to disable trimming.
    /// </summary>
    public int NewFilePerRunMaxFiles { get; init; } = 50;

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
