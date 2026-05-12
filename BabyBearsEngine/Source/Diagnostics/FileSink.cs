using System.IO;
using System.Threading;

namespace BabyBearsEngine.Diagnostics;

/// <param name="filePath">Destination file. Created on first write.</param>
/// <param name="preamble">Optional text written once, immediately before the first log entry of this sink's lifetime. Use to embed run-start banners without touching the file until there's something to log.</param>
internal sealed class FileSink(string filePath, string? preamble = null) : ILogSink
{
    private static readonly Lock s_lock = new();

    public void Write(LogLevel level, string message, Exception? exception)
    {
        string text = exception is null
            ? message + Environment.NewLine
            : message + Environment.NewLine + exception + Environment.NewLine;

        lock (s_lock)
        {
            if (preamble is not null)
            {
                File.AppendAllText(filePath, preamble);
                preamble = null;
            }

            File.AppendAllText(filePath, text);
        }
    }
}
