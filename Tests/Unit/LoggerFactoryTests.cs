using System.IO;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class LoggerFactoryTests
{
    private string _tempDir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "bbe-loggerfactory-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private string CreateLogFile(string name)
    {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, "log content");
        return path;
    }

    // TrimOldRunFiles — pure file enumeration / deletion, no Logger setup needed.

    [TestMethod]
    public void TrimOldRunFiles_FewerFilesThanMax_DeletesNothing()
    {
        CreateLogFile("log_2026-01-01_00-00-00.log");
        CreateLogFile("log_2026-01-02_00-00-00.log");

        LoggerFactory.TrimOldRunFiles(_tempDir, "log", ".log", maxFiles: 5);

        Assert.HasCount(2, Directory.GetFiles(_tempDir));
    }

    [TestMethod]
    public void TrimOldRunFiles_MoreFilesThanMax_DeletesOldestLeavingRoomForOneMore()
    {
        // maxFiles is the post-trim+write target. With 5 existing and max=3, we expect 2 to remain
        // so that after the next-run file is written the dir holds exactly 3.
        CreateLogFile("log_2026-01-01_00-00-00.log");
        CreateLogFile("log_2026-01-02_00-00-00.log");
        CreateLogFile("log_2026-01-03_00-00-00.log");
        CreateLogFile("log_2026-01-04_00-00-00.log");
        CreateLogFile("log_2026-01-05_00-00-00.log");

        LoggerFactory.TrimOldRunFiles(_tempDir, "log", ".log", maxFiles: 3);

        string[] remaining = Directory.GetFiles(_tempDir);
        Assert.HasCount(2, remaining);
        // The two newest should be the ones kept.
        Assert.IsTrue(remaining.Any(f => f.EndsWith("log_2026-01-04_00-00-00.log")));
        Assert.IsTrue(remaining.Any(f => f.EndsWith("log_2026-01-05_00-00-00.log")));
    }

    [TestMethod]
    public void TrimOldRunFiles_MaxZero_DisablesTrimming()
    {
        CreateLogFile("log_2026-01-01_00-00-00.log");
        CreateLogFile("log_2026-01-02_00-00-00.log");

        LoggerFactory.TrimOldRunFiles(_tempDir, "log", ".log", maxFiles: 0);

        Assert.HasCount(2, Directory.GetFiles(_tempDir));
    }

    [TestMethod]
    public void TrimOldRunFiles_IgnoresUnrelatedFiles()
    {
        // Files that don't match the {name}_*.{ext} pattern must be left alone even when the
        // directory is over the limit overall.
        CreateLogFile("log_2026-01-01_00-00-00.log");
        CreateLogFile("log_2026-01-02_00-00-00.log");
        CreateLogFile("log_2026-01-03_00-00-00.log");
        CreateLogFile("log_2026-01-04_00-00-00.log");
        CreateLogFile("notes.txt");
        CreateLogFile("other_2026-01-01_00-00-00.log"); // wrong basename prefix
        CreateLogFile("log.log"); // base file without timestamp suffix

        LoggerFactory.TrimOldRunFiles(_tempDir, "log", ".log", maxFiles: 2);

        string[] remaining = Directory.GetFiles(_tempDir);
        // 4 log_*.log files trimmed down to 1 (leaving room for one more = 2 total after write).
        // The 3 unrelated files must survive.
        Assert.IsTrue(remaining.Any(f => f.EndsWith("notes.txt")));
        Assert.IsTrue(remaining.Any(f => f.EndsWith("other_2026-01-01_00-00-00.log")));
        Assert.IsTrue(remaining.Any(f => f.EndsWith("log.log")));
        int trimmedFamilyCount = remaining.Count(f => Path.GetFileName(f).StartsWith("log_") && f.EndsWith(".log"));
        Assert.AreEqual(1, trimmedFamilyCount);
    }
}
