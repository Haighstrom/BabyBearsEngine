using System.IO;

namespace BabyBearsEngine.Diagnostics;

/// <param name="filePath">Destination file. Created on first write.</param>
/// <param name="preamble">Optional text written once, immediately before the first log entry of this sink's lifetime. Use to embed run-start banners without touching the file until there's something to log.</param>
/// <remarks>
/// Holds a single AutoFlush'd <see cref="StreamWriter"/> for the lifetime of the sink so each
/// log entry doesn't pay an open-write-close round trip. AutoFlush keeps each line on disk
/// immediately, so a crash mid-frame still captures everything up to the last completed write.
/// Not internally thread-safe — the surrounding <see cref="Logger"/> serialises calls into the
/// sink via its own lock.
/// </remarks>
internal sealed class FileSink(string filePath, string? preamble = null) : ILogSink, IDisposable
{
    private StreamWriter? _writer;
    private bool _disposed;

    public void Write(LogLevel level, string message, Exception? exception)
    {
        if (_disposed)
        {
            return;
        }

        StreamWriter writer = _writer ??= OpenWriter();

        if (preamble is not null)
        {
            writer.Write(preamble);
            preamble = null;
        }

        writer.WriteLine(message);
        if (exception is not null)
        {
            writer.WriteLine(exception);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _writer?.Dispose();
        _writer = null;
    }

    private StreamWriter OpenWriter()
    {
        // FileShare.ReadWrite so external tools (Notepad, tail, VS, File.ReadAllText) can open
        // the live log with their default FileShare without needing to know we hold it for
        // writing — File.ReadAllText defaults to FileShare.Read, which can't coexist with a
        // FileAccess.Write opener that demands FileShare.Read back. AutoFlush=true keeps each
        // line crash-safe at the cost of a syscall per line — fine for typical log volume.
        FileStream stream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        return new StreamWriter(stream) { AutoFlush = true };
    }
}
