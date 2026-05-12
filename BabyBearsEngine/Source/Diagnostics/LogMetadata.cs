namespace BabyBearsEngine;

/// <summary>
/// Flags controlling which metadata fields are prefixed onto each log message. Applies to all sinks
/// (console, file) so output stays consistent across destinations.
/// </summary>
[Flags]
public enum LogMetadata
{
    /// <summary>No metadata prefix; the raw message only.</summary>
    None = 0,

    /// <summary>Prefix each message with its severity, e.g. <c>[Warning]</c>.</summary>
    LogLevel = 1 << 0,

    /// <summary>Prefix each message with the wall-clock time, e.g. <c>[09:14:32.123]</c>.</summary>
    Timestamp = 1 << 1,

    /// <summary>Prefix each message with the source location of the caller, e.g. <c>[FileName.cs:42 MemberName]</c>.</summary>
    CallerInfo = 1 << 2,

    /// <summary>Shorthand for <see cref="LogLevel"/> and <see cref="Timestamp"/> (the everyday combo).</summary>
    Default = LogLevel | Timestamp,
}
