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
    /// <summary>The fully-constructed set of sinks for a logger run. Any sink slot may be null if the matching settings disable that destination. <see cref="Banner"/> is the per-run banner text; <see cref="Logger.Initialise"/> emits it to console + main file via normal writes (so both destinations show it), while the error file receives it as a lazy preamble so an empty run leaves no errors.log on disk.</summary>
    internal readonly record struct LoggerSinks(ConsoleSink? Console, FileSink? File, FileSink? ErrorFile, string Banner);

    public static LoggerSinks BuildSinks(LogSettings logSettings, ConsoleSettings consoleSettings)
    {
        string banner = BuildRunBanner();

        return new LoggerSinks(
            Console: BuildConsoleSink(logSettings, consoleSettings),
            File: BuildFileSink(logSettings),
            ErrorFile: BuildErrorFileSink(logSettings, banner),
            Banner: banner);
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

        ErrorFileTrimmer.ArchivePreviousRun(
            logSettings.ErrorFilePath,
            logSettings.ErrorArchivePath,
            logSettings.ErrorArchiveMaxRuns);

        return new FileSink(logSettings.ErrorFilePath, banner);
    }

    private static FileSink? BuildFileSink(LogSettings logSettings)
    {
        if (logSettings.FilePath is null || logSettings.FileLevels == LogLevel.None)
        {
            return null;
        }

        string? resolvedPath = ResolveFilePath(logSettings.FilePath, logSettings.FileMode, logSettings.NewFilePerRunMaxFiles);
        // No preamble — the banner is written by Logger.Initialise via a normal write so it
        // reaches both console and file sinks. The error file still uses a preamble for laziness.
        return resolvedPath is null ? null : new FileSink(resolvedPath);
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

    private static string? ResolveFilePath(string basePath, LogFileMode mode, int newFilePerRunMaxFiles)
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
                TrimOldRunFiles(dir, name, ext, newFilePerRunMaxFiles);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                return Path.Combine(dir, $"{name}_{timestamp}{ext}");

            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    /// <summary>
    /// Deletes oldest <c>{name}_*{ext}</c> files in <paramref name="dir"/> so that, after the
    /// caller writes the next run's file, the directory holds at most <paramref name="maxFiles"/>
    /// matching files. No-op when <paramref name="maxFiles"/> is ≤ 0 or the directory doesn't
    /// exist. Files are ranked by filename — the timestamp suffix sorts lexicographically.
    /// </summary>
    internal static void TrimOldRunFiles(string dir, string name, string ext, int maxFiles)
    {
        if (maxFiles <= 0 || string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
        {
            return;
        }

        string[] matching = Directory.GetFiles(dir, $"{name}_*{ext}");
        if (matching.Length + 1 <= maxFiles)
        {
            return;
        }

        Array.Sort(matching, StringComparer.Ordinal);
        int toDelete = matching.Length + 1 - maxFiles;
        for (int i = 0; i < toDelete; i++)
        {
            try
            {
                File.Delete(matching[i]);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Best-effort cleanup — if a file is locked by another process we just leave it
                // and let the trim catch up on a future run.
            }
        }
    }
}
