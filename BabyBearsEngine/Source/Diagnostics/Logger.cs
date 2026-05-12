using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Static facade for diagnostic logging. Call <see cref="Initialise"/> at startup to bind to
/// <see cref="LogSettings"/> and <see cref="ConsoleSettings"/>; until then, defaults apply.
/// Severity wrappers (<see cref="Information"/>, <see cref="Warning"/>, etc.) auto-capture caller
/// source location — the prefix is only emitted when <see cref="LogMetadata.CallerInfo"/> is set
/// in <see cref="LogSettings.MessageMetadata"/>. <see cref="SectionHeader"/>, <see cref="SectionBreak"/>
/// and <see cref="NewLine"/> bypass metadata for clean visual dividers in the log stream.
///
/// <see cref="GLError"/> is a specialised entrypoint for OpenGL errors that deduplicates by source
/// location when <see cref="LogSettings.DedupeGLErrors"/> is true (the default), preventing a
/// persistent render-loop failure from filling the log with one entry per frame.
/// Call <see cref="ResetDedupe"/> to forget seen locations.
/// </summary>
public static class Logger
{
    private const int SectionWidth = 70;

    private static readonly Lock s_lock = new();
    private static readonly HashSet<(string CallerFilePath, int CallerLineNumber)> s_dedupeKeys = [];

    private static ConsoleSink? s_consoleSink;
    private static FileSink? s_errorFileSink;
    private static FileSink? s_fileSink;
    private static LogSettings s_settings = LogSettings.Default;

    static Logger()
    {
        var sinks = LoggerFactory.BuildSinks(LogSettings.Default, ConsoleSettings.Default);
        s_consoleSink = sinks.Console;
        s_fileSink = sinks.File;
        s_errorFileSink = sinks.ErrorFile;
    }

    /// <summary>
    /// Reconfigures the logger from the given settings. Clears the dedupe set so each run starts
    /// fresh. Call once at application startup (e.g. from <c>GameLauncher.Run</c>). Not thread-safe
    /// with concurrent log calls — invoke before any worker threads start logging.
    /// </summary>
    public static void Initialise(LogSettings logSettings, ConsoleSettings consoleSettings)
    {
        ArgumentNullException.ThrowIfNull(logSettings);
        ArgumentNullException.ThrowIfNull(consoleSettings);

        lock (s_lock)
        {
            s_settings = logSettings;
            var sinks = LoggerFactory.BuildSinks(logSettings, consoleSettings);
            s_consoleSink = sinks.Console;
            s_fileSink = sinks.File;
            s_errorFileSink = sinks.ErrorFile;
            s_dedupeKeys.Clear();

            // Emit the per-run banner to console + main file. The error file already received it
            // as a lazy preamble (won't appear until the first error fires). Trim trailing newlines
            // because the sinks add their own.
            string bannerLine = sinks.Banner.TrimEnd('\r', '\n');
            s_consoleSink?.Write(LogLevel.Information, bannerLine, exception: null);
            s_fileSink?.Write(LogLevel.Information, bannerLine, exception: null);
        }
    }

    /// <summary>
    /// Clears the deduplication set used by <see cref="GLError"/>. After calling this, the next
    /// <see cref="GLError"/> call from each previously-seen source location will log again — useful
    /// after a shader reload or other recovery flow when you want fresh visibility into whether
    /// errors are still firing. No effect when <see cref="LogSettings.DedupeGLErrors"/> is false.
    /// <see cref="Initialise"/> also clears the set.
    /// </summary>
    public static void ResetDedupe()
    {
        lock (s_lock)
        {
            s_dedupeKeys.Clear();
        }
    }

    public static void Verbose(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Verbose, message, exception: null, callerFilePath, callerLineNumber, callerMemberName);
    }

    public static void Debug(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Debug, message, exception: null, callerFilePath, callerLineNumber, callerMemberName);
    }

    public static void Info(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Information, message, exception: null, callerFilePath, callerLineNumber, callerMemberName);
    }

    public static void Information(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Information, message, exception: null, callerFilePath, callerLineNumber, callerMemberName);
    }

    public static void Warning(string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Warning, message, exception: null, callerFilePath, callerLineNumber, callerMemberName);
    }

    public static void Error(string message, Exception? exception = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Error, message, exception, callerFilePath, callerLineNumber, callerMemberName);
    }

    public static void Fatal(string message, Exception? exception = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        Write(LogLevel.Fatal, message, exception, callerFilePath, callerLineNumber, callerMemberName);
    }

    /// <summary>
    /// Logs an OpenGL error at <see cref="LogLevel.Error"/>, deduplicated by source location when
    /// <see cref="LogSettings.DedupeGLErrors"/> is true (the default). Use this from render-loop
    /// code where a persistent failure would otherwise produce one log entry per frame. A blacked-out
    /// window from a broken shader is often as user-facing as a crash, so these route through the
    /// error log alongside other <c>Error</c> output.
    /// </summary>
    public static void GLError(string message, Exception? exception = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        if (s_settings.DedupeGLErrors && !TryClaimDedupe(callerFilePath, callerLineNumber))
        {
            return;
        }

        Write(LogLevel.Error, message, exception, callerFilePath, callerLineNumber, callerMemberName);
    }

    /// <summary>Writes a visually-centred section header like <c>---------- Name ----------</c> with no metadata prefix. Routed only to console and main file sinks (never the error file).</summary>
    public static void SectionHeader(string name, LogLevel level = LogLevel.Information)
    {
        ArgumentNullException.ThrowIfNull(name);

        int padTotal = Math.Max(2, SectionWidth - name.Length - 2);
        int padLeft = padTotal / 2;
        int padRight = padTotal - padLeft;
        string text = $"{new string('-', padLeft)} {name} {new string('-', padRight)}";

        WriteRaw(level, text);
    }

    /// <summary>Writes a horizontal divider line (no metadata prefix). Routed only to console and main file sinks (never the error file).</summary>
    public static void SectionBreak(LogLevel level = LogLevel.Information)
    {
        WriteRaw(level, new string('-', SectionWidth));
    }

    /// <summary>Writes a blank line (no metadata prefix). Routed only to console and main file sinks (never the error file).</summary>
    public static void NewLine(LogLevel level = LogLevel.Information)
    {
        WriteRaw(level, string.Empty);
    }

    /// <summary>
    /// Writes a banner-style block: an opening <c>===</c> divider, a title line, then arbitrary
    /// body lines. Matches the visual style of the run banner. Use for diagnostic info dumps where
    /// the content should stand out from normal log entries (engine setup, system info, etc.).
    /// Routed only to console and main file sinks (never the error file).
    /// </summary>
    /// <remarks>
    /// Sections do not emit a closing divider — consecutive sections share their dividers (each
    /// section's opening line closes the previous one). Call <see cref="SectionDivider"/> after the
    /// last section to add a closing divider.
    /// </remarks>
    public static void Section(string title, IEnumerable<string> lines, LogLevel level = LogLevel.Information)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(lines);

        if (!IsRawWriteEnabled(level))
        {
            return;
        }

        WriteRaw(level, new string('=', SectionWidth));
        WriteRaw(level, $" {title}");
        foreach (string line in lines)
        {
            WriteRaw(level, $" {line}");
        }
    }

    /// <summary>
    /// Writes a single <c>===</c> divider line matching the visual style of <see cref="Section"/>.
    /// Use after the final section to close off a series of sections, or as a heavyweight separator
    /// elsewhere. Routed only to console and main file sinks (never the error file).
    /// </summary>
    public static void SectionDivider(LogLevel level = LogLevel.Information)
    {
        WriteRaw(level, new string('=', SectionWidth));
    }

    /// <summary>
    /// Writes a single-line section marker like <c>=========== Text ===========</c>. Use for short
    /// "phase complete" annotations between sections. Routed only to console and main file sinks
    /// (never the error file).
    /// </summary>
    public static void SectionMarker(string text, LogLevel level = LogLevel.Information)
    {
        ArgumentNullException.ThrowIfNull(text);

        int padTotal = Math.Max(2, SectionWidth - text.Length - 2);
        int padLeft = padTotal / 2;
        int padRight = padTotal - padLeft;
        WriteRaw(level, $"{new string('=', padLeft)} {text} {new string('=', padRight)}");
    }

    private static string FormatLevel(LogLevel level) => level switch
    {
        LogLevel.Verbose => "[Verbose] ",
        LogLevel.Debug => "[Debug] ",
        LogLevel.Information => "[Information] ",
        LogLevel.Warning => "[Warning] ",
        LogLevel.Error => "[Error] ",
        LogLevel.Fatal => "[Fatal] ",
        _ => string.Empty,
    };

    private static string FormatMessage(LogLevel level, string message, string callerFilePath, int callerLineNumber, string callerMemberName)
    {
        var prefix = string.Empty;

        if (s_settings.MessageMetadata.HasFlag(LogMetadata.Timestamp))
        {
            prefix += $"[{DateTime.Now:HH:mm:ss.fff}] ";
        }

        if (s_settings.MessageMetadata.HasFlag(LogMetadata.LogLevel))
        {
            prefix += FormatLevel(level);
        }

        if (s_settings.MessageMetadata.HasFlag(LogMetadata.CallerInfo))
        {
            string file = string.IsNullOrEmpty(callerFilePath) ? "?" : Path.GetFileName(callerFilePath);
            prefix += $"[{file}:{callerLineNumber} {callerMemberName}] ";
        }

        return prefix + message;
    }

    private static bool IsRawWriteEnabled(LogLevel level)
    {
        return (s_consoleSink is not null && s_settings.ConsoleLevels.HasFlag(level))
            || (s_fileSink is not null && s_settings.FileLevels.HasFlag(level));
    }

    private static bool TryClaimDedupe(string callerFilePath, int callerLineNumber)
    {
        lock (s_lock)
        {
            return s_dedupeKeys.Add((callerFilePath, callerLineNumber));
        }
    }

    private static void Write(LogLevel level, string message, Exception? exception, string callerFilePath, int callerLineNumber, string callerMemberName)
    {
        bool consoleAccepts = s_consoleSink is not null && s_settings.ConsoleLevels.HasFlag(level);
        bool fileAccepts = s_fileSink is not null && s_settings.FileLevels.HasFlag(level);
        bool errorFileAccepts = s_errorFileSink is not null && s_settings.ErrorFileLevels.HasFlag(level);

        if (!consoleAccepts && !fileAccepts && !errorFileAccepts)
        {
            return;
        }

        string formatted = FormatMessage(level, message, callerFilePath, callerLineNumber, callerMemberName);

        if (consoleAccepts)
        {
            s_consoleSink!.Write(level, formatted, exception);
        }
        if (fileAccepts)
        {
            s_fileSink!.Write(level, formatted, exception);
        }
        if (errorFileAccepts)
        {
            s_errorFileSink!.Write(level, formatted, exception);
        }
    }

    private static void WriteRaw(LogLevel level, string message)
    {
        if (s_consoleSink is not null && s_settings.ConsoleLevels.HasFlag(level))
        {
            s_consoleSink.Write(level, message, exception: null);
        }
        if (s_fileSink is not null && s_settings.FileLevels.HasFlag(level))
        {
            s_fileSink.Write(level, message, exception: null);
        }
    }
}
