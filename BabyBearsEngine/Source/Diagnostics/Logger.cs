using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

// CA2254/CA1873 push users toward structured logging templates and IsEnabled guards. This wrapper
// is intentionally interpolation-based (see notes on lazy/structured formatting in design docs),
// so both rules are suppressed for the wrapper file only.
#pragma warning disable CA2254 // Template should be a static expression
#pragma warning disable CA1873 // Avoid potentially expensive logging methods

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Static facade over <see cref="ILogger"/>. Use <see cref="Initialise"/> at startup to bind to
/// <see cref="LogSettings"/> and <see cref="ConsoleSettings"/>; until then, defaults apply.
/// Severity wrappers (<see cref="Info"/>, <see cref="Warning"/>, etc.) auto-capture caller source
/// location — call from anywhere; the prefix is only emitted when
/// <see cref="LogSettings.IncludeCallerInfo"/> is true.
/// </summary>
public static class Logger
{
    private static readonly Lock s_lock = new();
    private static ILoggerFactory s_factory = CreateFactory(LogSettings.Default, ConsoleSettings.Default);
    private static ILogger s_logger = s_factory.CreateLogger("BabyBearsEngine");
    private static LogSettings s_settings = LogSettings.Default;

    private static ILoggerFactory CreateFactory(LogSettings logSettings, ConsoleSettings consoleSettings)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(logSettings.MinimumLevel);

            if (logSettings.MinimumLevel == LogLevel.None)
            {
                return;
            }

            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.ColorBehavior = consoleSettings.ColouriseLogOutput
                    ? LoggerColorBehavior.Enabled
                    : LoggerColorBehavior.Disabled;
                options.TimestampFormat = consoleSettings.IncludeLogTimestamp ? "[HH:mm:ss.fff] " : null;
            });

            if (logSettings.FilePath is { } path)
            {
                builder.AddProvider(new FileLoggerProvider(path));
            }
        });
    }

    /// <summary>
    /// Reconfigures the logger from the given settings. Disposes the previous factory.
    /// Call once at application startup (e.g. from <c>GameLauncher.Run</c>). Not thread-safe with
    /// concurrent log calls — invoke before any worker threads start logging.
    /// </summary>
    public static void Initialise(LogSettings logSettings, ConsoleSettings consoleSettings)
    {
        ArgumentNullException.ThrowIfNull(logSettings);
        ArgumentNullException.ThrowIfNull(consoleSettings);

        lock (s_lock)
        {
            s_factory.Dispose();
            s_factory = CreateFactory(logSettings, consoleSettings);
            s_logger = s_factory.CreateLogger("BabyBearsEngine");
            s_settings = logSettings;
        }
    }

    private static string Prefix(string message, string callerFilePath, int callerLineNumber, string callerMemberName)
    {
        if (!s_settings.IncludeCallerInfo)
        {
            return message;
        }

        string file = string.IsNullOrEmpty(callerFilePath) ? "?" : Path.GetFileName(callerFilePath);
        return $"[{file}:{callerLineNumber} {callerMemberName}] {message}";
    }

    public static void Trace(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        s_logger.LogTrace(Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
    }

    public static void Debug(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        s_logger.LogDebug(Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
    }

    public static void Info(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        s_logger.LogInformation(Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
    }

    public static void Warning(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        s_logger.LogWarning(Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
    }

    public static void Error(string message, Exception? exception = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        if (exception is null)
        {
            s_logger.LogError(Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
        }
        else
        {
            s_logger.LogError(exception, Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
        }
    }

    public static void Critical(string message, Exception? exception = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        if (exception is null)
        {
            s_logger.LogCritical(Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
        }
        else
        {
            s_logger.LogCritical(exception, Prefix(message, callerFilePath, callerLineNumber, callerMemberName));
        }
    }
}
