using System.IO;
using System.Reflection;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Builds the sink set used by <see cref="Logger"/> from <see cref="LogSettings"/> and
/// <see cref="ConsoleSettings"/>. Pure factory — no static state of its own; the timestamp inside
/// each run banner is captured once per <see cref="BuildSinks"/> call so both file sinks share a
/// single, consistent run header. The banner only contains the "this run started, here is the
/// engine" intro — diagnostic detail (system info, GPU, displays) is emitted as separate sections
/// from <see cref="EngineDiagnostics"/>.
/// </summary>
internal static class LoggerFactory
{
    /// <summary>The fully-constructed set of sinks for a logger run. Any slot may be null if the matching settings disable that destination.</summary>
    internal readonly record struct LoggerSinks(ConsoleSink? Console, FileSink? File, FileSink? ErrorFile);

    public static LoggerSinks BuildSinks(LogSettings logSettings, ConsoleSettings consoleSettings)
    {
        string banner = BuildRunBanner();

        return new LoggerSinks(
            Console: BuildConsoleSink(logSettings, consoleSettings),
            File: BuildFileSink(logSettings, banner),
            ErrorFile: BuildErrorFileSink(logSettings, banner));
    }

    private static ConsoleSink? BuildConsoleSink(LogSettings logSettings, ConsoleSettings consoleSettings)
    {
        return logSettings.ConsoleLevels == LogLevel.None
            ? null
            : new ConsoleSink(consoleSettings.ColouriseLogOutput);
    }

    private static FileSink? BuildErrorFileSink(LogSettings logSettings, string banner)
    {
        if (logSettings.ErrorFilePath is null || logSettings.ErrorFileLevels == LogLevel.None)
        {
            return null;
        }

        return new FileSink(logSettings.ErrorFilePath, banner);
    }

    private static FileSink? BuildFileSink(LogSettings logSettings, string banner)
    {
        if (logSettings.FilePath is null || logSettings.FileLevels == LogLevel.None)
        {
            return null;
        }

        string? resolvedPath = ResolveFilePath(logSettings.FilePath, logSettings.FileMode);
        return resolvedPath is null ? null : new FileSink(resolvedPath, banner);
    }

    private static string BuildRunBanner()
    {
        // InformationalVersion can include a SourceLink suffix like "0.1.0+commitHash"; strip it.
        string version = (typeof(LoggerFactory).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(LoggerFactory).Assembly.GetName().Version?.ToString()
            ?? "unknown")
            .Split('+')[0];
        string game = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";

        const int width = 70;
        string sep = new('=', width);
        string nl = Environment.NewLine;

        const string markerText = "Powered by BabyBearsEngine";
        int padTotal = Math.Max(2, width - markerText.Length - 2);
        int padLeft = padTotal / 2;
        int padRight = padTotal - padLeft;
        string marker = $"{new string('=', padLeft)} {markerText} {new string('=', padRight)}";

        return $"{marker}{nl}"
             + $"{nl}"
             + $"{sep}{nl}"
             + $" Run Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{nl}"
             + $" Game: {game}{nl}"
             + $" BabyBearsEngine v{version}{nl}"
             + $"{sep}{nl}";
    }

    private static string? ResolveFilePath(string basePath, LogFileMode mode)
    {
        switch (mode)
        {
            case LogFileMode.AppendToExisting:
                return basePath;

            case LogFileMode.OverwriteExisting:
                if (File.Exists(basePath))
                {
                    File.Delete(basePath);
                }
                return basePath;

            case LogFileMode.NewFilePerRun:
                string dir = Path.GetDirectoryName(basePath) ?? string.Empty;
                string name = Path.GetFileNameWithoutExtension(basePath);
                string ext = Path.GetExtension(basePath);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                return Path.Combine(dir, $"{name}_{timestamp}{ext}");

            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
