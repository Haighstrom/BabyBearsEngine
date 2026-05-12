namespace BabyBearsEngine;

/// <summary>
/// Severity levels used by <see cref="Diagnostics.Logger"/>. Each individual severity is a single
/// bit, so values combine to form filter masks; <see cref="LogSettings"/> uses these masks to decide
/// which levels go to which sink and which levels deduplicate.
/// </summary>
/// <remarks>
/// When passed as the level of a single log call (e.g. internally by <c>Logger.Warning</c>) only the
/// individual-severity values are meaningful. The composite presets (<see cref="All"/>,
/// <see cref="WarningAndAbove"/>, etc.) exist purely for setting filter masks in
/// <see cref="LogSettings"/>; passing a composite where a single severity is expected has no
/// meaningful interpretation.
/// </remarks>
[Flags]
public enum LogLevel
{
    /// <summary>The empty mask; matches no severities. Used to disable a sink or feature entirely.</summary>
    None = 0,

    /// <summary>Very chatty diagnostic detail; off by default.</summary>
    Verbose = 1 << 0,

    /// <summary>Diagnostic info useful during development.</summary>
    Debug = 1 << 1,

    /// <summary>Normal application flow.</summary>
    Information = 1 << 2,

    /// <summary>Something unexpected but recoverable.</summary>
    Warning = 1 << 3,

    /// <summary>Operation failed; an exception is typically attached.</summary>
    Error = 1 << 4,

    /// <summary>Unrecoverable failure; the application is about to crash or has lost a major subsystem.</summary>
    Fatal = 1 << 5,

    /// <summary>All severities.</summary>
    All = Verbose | Debug | Information | Warning | Error | Fatal,

    /// <summary><see cref="Information"/> and above (excludes <see cref="Verbose"/> and <see cref="Debug"/>).</summary>
    InformationAndAbove = Information | Warning | Error | Fatal,

    /// <summary><see cref="Warning"/> and above. Common dedupe target — Errors and Warnings often spam in render/update loops.</summary>
    WarningAndAbove = Warning | Error | Fatal,

    /// <summary><see cref="Error"/> and above. Default target for the dedicated error-only log file.</summary>
    ErrorAndAbove = Error | Fatal,

    /// <summary>Everything except <see cref="Error"/> and <see cref="Fatal"/>. Useful when errors go to a separate file and the main log shouldn't duplicate them.</summary>
    AllButErrors = Verbose | Debug | Information | Warning,
}
