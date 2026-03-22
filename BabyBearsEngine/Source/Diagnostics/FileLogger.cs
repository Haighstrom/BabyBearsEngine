using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace BabyBearsEngine.Source.Diagnostics;

public sealed class FileLogger(string filePath) : ILogger
{
    private static readonly Lock s_lock = new();

    private readonly string _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message) && exception is null)
        {
            return;
        }

        var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                        $"[{logLevel}] " +
                        $"{message}";

        if (exception is not null)
        {
            logRecord += Environment.NewLine + exception;
        }

        logRecord += Environment.NewLine;

        lock (s_lock)
        {
            File.AppendAllText(_filePath, logRecord);
        }
    }
}
