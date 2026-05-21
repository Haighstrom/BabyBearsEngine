using System;
using System.IO;
using System.Text;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ErrorFileTrimmerTests
{
    private string _tempDir = null!;
    private string _errorsPath = null!;
    private string _archivePath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "bbe_errortrimmer_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _errorsPath = Path.Combine(_tempDir, "errors.log");
        _archivePath = Path.Combine(_tempDir, "error_archive.log");
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Windows file locks can outlive the test; not fatal.
        }
    }

    private string ReadErrors() => File.ReadAllText(_errorsPath);
    private string ReadArchive() => File.ReadAllText(_archivePath);

    private static string MakeRun(int runNumber)
    {
        StringBuilder sb = new();
        sb.AppendLine($"====== {ErrorFileTrimmer.RunStartMarker} ======");
        sb.AppendLine($" Run Started: 2024-01-01 00:00:{runNumber:D2}");
        sb.AppendLine($"[Fatal] Crash in run {runNumber:D3}");
        return sb.ToString();
    }

    // Builds an archive as it would look after N runs have been prepended: newest run first.
    private static string MakeArchive(int runCount)
    {
        StringBuilder sb = new();
        for (int runNumber = runCount; runNumber >= 1; runNumber--)
        {
            sb.Append(MakeRun(runNumber));
        }
        return sb.ToString();
    }

    // ─── No-op cases ───

    [TestMethod]
    public void ArchivePreviousRun_ErrorsFileDoesNotExist_DoesNothing()
    {
        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 50);

        Assert.IsFalse(File.Exists(_errorsPath));
        Assert.IsFalse(File.Exists(_archivePath));
    }

    [TestMethod]
    public void ArchivePreviousRun_ErrorsFileHasNoRunMarker_DeletesFileWithoutArchiving()
    {
        File.WriteAllText(_errorsPath, "some content without the marker");

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 50);

        Assert.IsFalse(File.Exists(_errorsPath));
        Assert.IsFalse(File.Exists(_archivePath));
    }

    [TestMethod]
    public void ArchivePreviousRun_NullArchivePath_DeletesErrorsFileWithoutArchiving()
    {
        File.WriteAllText(_errorsPath, MakeRun(1));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, archivePath: null, maxArchiveRuns: 50);

        Assert.IsFalse(File.Exists(_errorsPath));
        Assert.IsFalse(File.Exists(_archivePath));
    }

    // ─── Archive creation ───

    [TestMethod]
    public void ArchivePreviousRun_ErrorsFileHasRun_ArchiveDoesNotExist_CreatesArchiveWithRun()
    {
        File.WriteAllText(_errorsPath, MakeRun(1));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 50);

        Assert.IsTrue(File.Exists(_archivePath));
        Assert.Contains("Crash in run 001", ReadArchive());
    }

    [TestMethod]
    public void ArchivePreviousRun_ErrorsFileHasRun_DeletesErrorsFile()
    {
        File.WriteAllText(_errorsPath, MakeRun(1));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 50);

        Assert.IsFalse(File.Exists(_errorsPath));
    }

    // ─── Prepend order ───

    [TestMethod]
    public void ArchivePreviousRun_ArchiveExists_NewRunPrependedBeforeOldContent()
    {
        File.WriteAllText(_errorsPath, MakeRun(2));
        File.WriteAllText(_archivePath, MakeRun(1));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 50);

        string archive = ReadArchive();
        int run2Position = archive.IndexOf("Crash in run 002", StringComparison.Ordinal);
        int run1Position = archive.IndexOf("Crash in run 001", StringComparison.Ordinal);
        Assert.IsLessThan(run1Position, run2Position, "Newer run must appear before older run in archive.");
    }

    [TestMethod]
    public void ArchivePreviousRun_ArchiveExists_BothRunsPresent()
    {
        File.WriteAllText(_errorsPath, MakeRun(2));
        File.WriteAllText(_archivePath, MakeRun(1));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 50);

        string archive = ReadArchive();
        Assert.Contains("Crash in run 001", archive);
        Assert.Contains("Crash in run 002", archive);
    }

    // ─── Trimming ───

    [TestMethod]
    public void ArchivePreviousRun_ArchiveBelowMaxRuns_NotTrimmed()
    {
        File.WriteAllText(_errorsPath, MakeRun(6));
        File.WriteAllText(_archivePath, MakeArchive(runCount: 5));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 10);

        Assert.AreEqual(6, CountOccurrences(ReadArchive(), ErrorFileTrimmer.RunStartMarker));
    }

    [TestMethod]
    public void ArchivePreviousRun_ArchiveExceedsMaxRuns_TrimsToMaxRuns()
    {
        File.WriteAllText(_errorsPath, MakeRun(11));
        File.WriteAllText(_archivePath, MakeArchive(runCount: 10));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 10);

        Assert.AreEqual(10, CountOccurrences(ReadArchive(), ErrorFileTrimmer.RunStartMarker));
    }

    [TestMethod]
    public void ArchivePreviousRun_ArchiveExceedsMaxRuns_OldestRunRemoved()
    {
        // Archive has runs 10..1 (newest first); errors.log has run 11.
        // Combined = [11, 10, ..., 1] = 11 runs; trimmed to 10 = [11, 10, ..., 2]. Run 1 dropped.
        File.WriteAllText(_errorsPath, MakeRun(11));
        File.WriteAllText(_archivePath, MakeArchive(runCount: 10));

        ErrorFileTrimmer.ArchivePreviousRun(_errorsPath, _archivePath, maxArchiveRuns: 10);

        string archive = ReadArchive();
        Assert.Contains("Crash in run 011", archive);
        Assert.DoesNotContain("Crash in run 001", archive);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0;
        int searchFrom = 0;
        while ((searchFrom = haystack.IndexOf(needle, searchFrom, StringComparison.Ordinal)) != -1)
        {
            count++;
            searchFrom += needle.Length;
        }
        return count;
    }
}
