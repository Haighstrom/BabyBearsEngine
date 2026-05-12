namespace BabyBearsEngine;

/// <summary>
/// Controls how <see cref="Diagnostics.Logger"/> handles the log file path at startup.
/// </summary>
public enum LogFileMode
{
    /// <summary>The log file is reused across runs and new content is appended. Existing content is preserved; file grows unboundedly over time.</summary>
    AppendToExisting,

    /// <summary>The log file is wiped at startup. A single file always contains the most recent run's output.</summary>
    OverwriteExisting,

    /// <summary>A new timestamped file is created per run (e.g. <c>log_2026-05-12_09-14-32.txt</c>). Previous files are preserved — no automatic cleanup is performed.</summary>
    NewFilePerRun,
}
