namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// A destination for log output. Implementations are owned by <see cref="Logger"/> and receive
/// pre-formatted message strings with metadata already applied.
/// </summary>
internal interface ILogSink
{
    /// <summary>Writes a pre-formatted log line. Level is supplied so sinks can colourise or filter further.</summary>
    void Write(LogLevel level, string message, Exception? exception);
}
