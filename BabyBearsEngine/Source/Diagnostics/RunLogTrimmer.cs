using System.IO;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Manages a run-overwritten log's archive. At startup, moves the previous run's log file into
/// its archive (prepended, so newest is at the top), trims the archive to a configured maximum,
/// then deletes the source file so the new run starts clean. Used for both <c>errors.log</c> /
/// <c>error_archive.log</c> and <c>log.log</c> / <c>log_archive.log</c>.
/// </summary>
internal static class RunLogTrimmer
{
    /// <summary>Unique text present in every run-start banner line.</summary>
    internal const string RunStartMarker = "Powered by BabyBearsEngine";

    /// <summary>
    /// Prepends the content of <paramref name="sourcePath"/> to <paramref name="archivePath"/>,
    /// trims the archive to <paramref name="maxArchiveRuns"/> runs, then deletes
    /// <paramref name="sourcePath"/>. Does nothing if the source file does not exist. If
    /// <paramref name="archivePath"/> is null the source file is still deleted but no archive
    /// is written.
    /// </summary>
    internal static void ArchivePreviousRun(string sourcePath, string? archivePath, int maxArchiveRuns)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        string previousRun = File.ReadAllText(sourcePath);
        bool hasRunMarker = previousRun.Contains(RunStartMarker, StringComparison.Ordinal);

        if (archivePath is not null && hasRunMarker)
        {
            string existingArchive = File.Exists(archivePath) ? File.ReadAllText(archivePath) : string.Empty;
            string combined = previousRun + existingArchive;
            File.WriteAllText(archivePath, TrimToFirstNRuns(combined, maxArchiveRuns));
        }
        else if (!hasRunMarker && previousRun.Length > 0)
        {
            // No banner means the file was truncated, externally edited, or written by a non-banner
            // sink — we have no safe way to identify run boundaries for the archive, so we discard.
            // Surface this in the log so the disappearance isn't silent if the user later goes
            // looking for prior entries.
            Logger.Warning($"Discarding log file '{sourcePath}' ({previousRun.Length} chars) because it contains no run-start banner; cannot archive without run boundaries.");
        }

        File.Delete(sourcePath);
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
