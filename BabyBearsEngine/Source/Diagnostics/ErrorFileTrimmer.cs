using System.IO;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Manages the error log archive. At startup, moves the previous run's <c>errors.log</c> into
/// <c>error_archive.log</c> (prepended, so newest is at the top), trims the archive to a
/// configured maximum, then deletes <c>errors.log</c> so the new run starts clean.
/// </summary>
internal static class ErrorFileTrimmer
{
    /// <summary>Unique text present in every run-start banner line.</summary>
    internal const string RunStartMarker = "Powered by BabyBearsEngine";

    /// <summary>
    /// Prepends the content of <paramref name="errorsPath"/> to <paramref name="archivePath"/>,
    /// trims the archive to <paramref name="maxArchiveRuns"/> runs, then deletes
    /// <paramref name="errorsPath"/>. Does nothing if the errors file does not exist. If
    /// <paramref name="archivePath"/> is null the errors file is still deleted but no archive
    /// is written.
    /// </summary>
    internal static void ArchivePreviousRun(string errorsPath, string? archivePath, int maxArchiveRuns)
    {
        if (!File.Exists(errorsPath))
        {
            return;
        }

        string previousRun = File.ReadAllText(errorsPath);

        if (archivePath is not null && previousRun.Contains(RunStartMarker, StringComparison.Ordinal))
        {
            string existingArchive = File.Exists(archivePath) ? File.ReadAllText(archivePath) : string.Empty;
            string combined = previousRun + existingArchive;
            File.WriteAllText(archivePath, TrimToFirstNRuns(combined, maxArchiveRuns));
        }

        File.Delete(errorsPath);
    }

    private static string TrimToFirstNRuns(string content, int maxRuns)
    {
        List<int> runStarts = FindRunStarts(content);

        if (runStarts.Count <= maxRuns)
        {
            return content;
        }

        int firstExcessMarkerIndex = runStarts[maxRuns];
        int prevNewline = firstExcessMarkerIndex > 0
            ? content.LastIndexOf('\n', firstExcessMarkerIndex - 1)
            : -1;

        return content[..(prevNewline + 1)];
    }

    private static List<int> FindRunStarts(string content)
    {
        List<int> runStarts = [];
        int searchFrom = 0;

        while (true)
        {
            int runIndex = content.IndexOf(RunStartMarker, searchFrom, StringComparison.Ordinal);

            if (runIndex == -1)
            {
                break;
            }

            runStarts.Add(runIndex);
            searchFrom = runIndex + RunStartMarker.Length;
        }

        return runStarts;
    }
}
