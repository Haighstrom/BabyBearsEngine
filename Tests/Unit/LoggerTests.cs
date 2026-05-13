using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class LoggerTests
{
    private TextWriter _originalConsoleOut = null!;
    private StringWriter _capturedConsole = null!;
    private string _tempDir = null!;
    private string _logPath = null!;
    private string _errorLogPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _originalConsoleOut = Console.Out;
        _capturedConsole = new StringWriter();
        Console.SetOut(_capturedConsole);

        _tempDir = Path.Combine(Path.GetTempPath(), "bbe_logger_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _logPath = Path.Combine(_tempDir, "log.log");
        _errorLogPath = Path.Combine(_tempDir, "errors.log");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Logger.Initialise(LogSettings.Silent, ConsoleSettings.Default);
        Logger.ResetDedupe();
        Console.SetOut(_originalConsoleOut);
        _capturedConsole.Dispose();

        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Windows occasionally holds file locks briefly after test ends; not fatal.
        }
    }

    private LogSettings TestSettings(
        LogLevel consoleLevels = LogLevel.All,
        LogLevel fileLevels = LogLevel.All,
        LogLevel errorFileLevels = LogLevel.ErrorAndAbove,
        LogFileMode fileMode = LogFileMode.OverwriteExisting,
        LogMetadata metadata = LogMetadata.Default,
        bool dedupeGLErrors = true,
        string? filePath = null,
        string? errorFilePath = null) => new()
        {
            ConsoleLevels = consoleLevels,
            FileLevels = fileLevels,
            ErrorFileLevels = errorFileLevels,
            FilePath = filePath ?? _logPath,
            ErrorFilePath = errorFilePath ?? _errorLogPath,
            FileMode = fileMode,
            MessageMetadata = metadata,
            DedupeGLErrors = dedupeGLErrors,
        };

    private static ConsoleSettings PlainConsole() => new() { ColouriseLogOutput = false };

    private string ConsoleOutput => _capturedConsole.ToString();
    private string LogFileContent => File.Exists(_logPath) ? File.ReadAllText(_logPath) : string.Empty;
    private string ErrorFileContent => File.Exists(_errorLogPath) ? File.ReadAllText(_errorLogPath) : string.Empty;

    // ─── Severity wrappers ───

    [TestMethod]
    public void Verbose_EmittedWithVerbosePrefix()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Verbose("hello");

        StringAssert.Contains(ConsoleOutput, "[Verbose]");
        StringAssert.Contains(ConsoleOutput, "hello");
    }

    [TestMethod]
    public void Debug_EmittedWithDebugPrefix()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Debug("hello");

        StringAssert.Contains(ConsoleOutput, "[Debug]");
    }

    [TestMethod]
    public void Information_EmittedWithInformationPrefix()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Information("hello");

        StringAssert.Contains(ConsoleOutput, "[Information]");
    }

    [TestMethod]
    public void Info_IsAliasForInformation()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Info("hello");

        StringAssert.Contains(ConsoleOutput, "[Information]");
        StringAssert.Contains(ConsoleOutput, "hello");
    }

    [TestMethod]
    public void Warning_EmittedWithWarningPrefix()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Warning("uh oh");

        StringAssert.Contains(ConsoleOutput, "[Warning]");
        StringAssert.Contains(ConsoleOutput, "uh oh");
    }

    [TestMethod]
    public void Error_EmittedWithErrorPrefix()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Error("bad");

        StringAssert.Contains(ConsoleOutput, "[Error]");
        StringAssert.Contains(ConsoleOutput, "bad");
    }

    [TestMethod]
    public void Error_WithException_IncludesExceptionTextInOutput()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Error("crashed", new InvalidOperationException("boom"));

        StringAssert.Contains(ConsoleOutput, "InvalidOperationException");
        StringAssert.Contains(ConsoleOutput, "boom");
    }

    [TestMethod]
    public void Fatal_EmittedWithFatalPrefix()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Fatal("dead");

        StringAssert.Contains(ConsoleOutput, "[Fatal]");
    }

    [TestMethod]
    public void Fatal_WithException_IncludesExceptionTextInOutput()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Fatal("dead", new InvalidOperationException("kaboom"));

        StringAssert.Contains(ConsoleOutput, "InvalidOperationException");
        StringAssert.Contains(ConsoleOutput, "kaboom");
    }

    [TestMethod]
    public void GLError_EmittedAtErrorLevel()
    {
        Logger.Initialise(TestSettings(dedupeGLErrors: false), PlainConsole());

        Logger.GLError("gl bad");

        StringAssert.Contains(ConsoleOutput, "[Error]");
        StringAssert.Contains(ConsoleOutput, "gl bad");
    }

    // ─── Filter masks ───

    [TestMethod]
    public void ConsoleLevels_OnlyMatchingLevelsWritten()
    {
        Logger.Initialise(TestSettings(consoleLevels: LogLevel.WarningAndAbove), PlainConsole());

        Logger.Information("info-msg");
        Logger.Warning("warn-msg");
        Logger.Error("error-msg");

        Assert.IsFalse(ConsoleOutput.Contains("info-msg"));
        StringAssert.Contains(ConsoleOutput, "warn-msg");
        StringAssert.Contains(ConsoleOutput, "error-msg");
    }

    [TestMethod]
    public void FileLevels_OnlyMatchingLevelsWritten()
    {
        Logger.Initialise(TestSettings(fileLevels: LogLevel.AllButErrors), PlainConsole());

        Logger.Information("info-msg");
        Logger.Error("error-msg");

        StringAssert.Contains(LogFileContent, "info-msg");
        Assert.IsFalse(LogFileContent.Contains("error-msg"));
    }

    [TestMethod]
    public void ErrorFileLevels_OnlyMatchingLevelsWritten()
    {
        Logger.Initialise(TestSettings(errorFileLevels: LogLevel.ErrorAndAbove), PlainConsole());

        Logger.Information("info-msg");
        Logger.Error("error-msg");
        Logger.Fatal("fatal-msg");

        Assert.IsFalse(ErrorFileContent.Contains("info-msg"));
        StringAssert.Contains(ErrorFileContent, "error-msg");
        StringAssert.Contains(ErrorFileContent, "fatal-msg");
    }

    [TestMethod]
    public void ConsoleLevels_None_NoConsoleOutputForLogs()
    {
        Logger.Initialise(TestSettings(consoleLevels: LogLevel.None), PlainConsole());

        Logger.Information("nope");

        Assert.IsFalse(ConsoleOutput.Contains("nope"));
    }

    [TestMethod]
    public void FileLevels_None_LogFileNotWritten()
    {
        Logger.Initialise(TestSettings(fileLevels: LogLevel.None), PlainConsole());

        Logger.Information("nope");

        Assert.IsFalse(File.Exists(_logPath), "Main log file should not be created when FileLevels=None.");
    }

    [TestMethod]
    public void ErrorFileLevels_None_ErrorFileNotWritten()
    {
        Logger.Initialise(TestSettings(errorFileLevels: LogLevel.None), PlainConsole());

        Logger.Error("nope");

        Assert.IsFalse(File.Exists(_errorLogPath), "Error file should not be created when ErrorFileLevels=None.");
    }

    // ─── GLError dedupe ───

    [TestMethod]
    public void GLError_DedupeOn_SameLocationLoggedOnlyOnce()
    {
        Logger.Initialise(TestSettings(dedupeGLErrors: true), PlainConsole());

        EmitTwoGLErrorsFromSameLine();

        int count = CountOccurrences(ConsoleOutput, "spam");
        Assert.AreEqual(1, count, "Dedupe should suppress the second call from the same source line.");
    }

    private static void EmitTwoGLErrorsFromSameLine()
    {
        for (int i = 0; i < 2; i++)
        {
            Logger.GLError("spam");
        }
    }

    [TestMethod]
    public void GLError_DedupeOn_DifferentLocations_BothLogged()
    {
        Logger.Initialise(TestSettings(dedupeGLErrors: true), PlainConsole());

        Logger.GLError("first-site");
        Logger.GLError("second-site");

        StringAssert.Contains(ConsoleOutput, "first-site");
        StringAssert.Contains(ConsoleOutput, "second-site");
    }

    [TestMethod]
    public void GLError_DedupeOff_AllCallsLogged()
    {
        Logger.Initialise(TestSettings(dedupeGLErrors: false), PlainConsole());

        for (int i = 0; i < 3; i++)
        {
            Logger.GLError("nodedupe");
        }

        int count = CountOccurrences(ConsoleOutput, "nodedupe");
        Assert.AreEqual(3, count, "All three calls should appear when dedupe is disabled.");
    }

    [TestMethod]
    public void ResetDedupe_AllowsRepeatLogFromSameLocation()
    {
        Logger.Initialise(TestSettings(dedupeGLErrors: true), PlainConsole());

        Logger.GLError("once");
        Logger.ResetDedupe();
        Logger.GLError("once");

        int count = CountOccurrences(ConsoleOutput, "once");
        Assert.AreEqual(2, count, "After ResetDedupe the same call site should log again.");
    }

    // ─── Caller info ───

    [TestMethod]
    public void CallerInfo_FlagSet_PrefixIncludesFileAndLine()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.Default | LogMetadata.CallerInfo), PlainConsole());

        Logger.Information("msg");

        StringAssert.Contains(ConsoleOutput, "LoggerTests.cs:");
    }

    [TestMethod]
    public void CallerInfo_FlagUnset_NoCallerPrefix()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.Default), PlainConsole());

        Logger.Information("msg");

        Assert.IsFalse(ConsoleOutput.Contains("LoggerTests.cs"));
    }

    // ─── Metadata flags (timestamp / level prefix) ───

    [TestMethod]
    public void Metadata_LogLevelFlagUnset_NoLevelPrefix()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.Timestamp), PlainConsole());

        Logger.Warning("msg");

        Assert.IsFalse(ConsoleOutput.Contains("[Warning]"));
        StringAssert.Contains(ConsoleOutput, "msg");
    }

    [TestMethod]
    public void Metadata_TimestampFlagSet_PrefixHasClockTime()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.Timestamp), PlainConsole());

        Logger.Information("msg");

        // Timestamp format is "[HH:mm:ss.fff] " — match a digit followed by ':'.
        StringAssert.Matches(ConsoleOutput, new System.Text.RegularExpressions.Regex(@"\[\d{2}:\d{2}:\d{2}\.\d{3}\]"));
    }

    [TestMethod]
    public void Metadata_None_NoPrefixesAtAll()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.None), PlainConsole());

        Logger.Information("bare");

        Assert.IsFalse(ConsoleOutput.Contains("[Information]"));
        // Banner is still in output (always-on), but the test message should appear with no prefix.
        Assert.IsTrue(ConsoleOutput.Contains(Environment.NewLine + "bare") || ConsoleOutput.EndsWith("bare" + Environment.NewLine));
    }

    // ─── Section helpers ───

    [TestMethod]
    public void Section_EmitsOpeningDividerThenTitleAndLines_WithNoClosingDivider()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.None), PlainConsole());
        _capturedConsole.GetStringBuilder().Clear();

        Logger.Section("MyTitle", ["alpha", "beta"]);

        string output = ConsoleOutput.Trim();
        string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual(4, lines.Length, $"Expected 4 lines (divider + title + 2 body lines), got: {output}");
        Assert.IsTrue(lines[0].All(c => c == '='), "First line should be the opening divider.");
        StringAssert.Contains(lines[1], "MyTitle");
        StringAssert.Contains(lines[2], "alpha");
        StringAssert.Contains(lines[3], "beta");
    }

    [TestMethod]
    public void SectionDivider_EmitsSingleLineOfEquals()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.None), PlainConsole());
        _capturedConsole.GetStringBuilder().Clear();

        Logger.SectionDivider();

        string output = ConsoleOutput.Trim();
        Assert.IsTrue(output.All(c => c == '='), $"SectionDivider should output only '=' characters, got: {output}");
    }

    [TestMethod]
    public void SectionMarker_OutputContainsTextSurroundedByEquals()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.None), PlainConsole());
        _capturedConsole.GetStringBuilder().Clear();

        Logger.SectionMarker("hello");

        string output = ConsoleOutput.Trim();
        StringAssert.Contains(output, "hello");
        StringAssert.StartsWith(output, "=");
        StringAssert.EndsWith(output, "=");
    }

    [TestMethod]
    public void NewLine_EmitsEmptyLine()
    {
        Logger.Initialise(TestSettings(metadata: LogMetadata.None), PlainConsole());
        _capturedConsole.GetStringBuilder().Clear();

        Logger.NewLine();

        Assert.AreEqual(Environment.NewLine, ConsoleOutput, "NewLine should emit exactly one blank line.");
    }

    // ─── Banner ───

    [TestMethod]
    public void Initialise_EmitsBannerToConsoleAndMainFile()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        StringAssert.Contains(ConsoleOutput, "Powered by BabyBearsEngine");
        StringAssert.Contains(ConsoleOutput, "Run Started:");
        StringAssert.Contains(LogFileContent, "Powered by BabyBearsEngine");
        StringAssert.Contains(LogFileContent, "Run Started:");
    }

    [TestMethod]
    public void Initialise_BannerNotEmittedToErrorFile_BeforeAnyErrorFires()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Assert.IsFalse(File.Exists(_errorLogPath), "Error file should be lazy — no error yet, no file.");
    }

    [TestMethod]
    public void Initialise_BannerInErrorFile_OnFirstError()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Error("first");

        StringAssert.Contains(ErrorFileContent, "Powered by BabyBearsEngine");
        StringAssert.Contains(ErrorFileContent, "first");
    }

    [TestMethod]
    public void Initialise_BannerNotRepeated_OnSubsequentErrors()
    {
        Logger.Initialise(TestSettings(), PlainConsole());

        Logger.Error("first");
        Logger.Error("second");

        int bannerCount = CountOccurrences(ErrorFileContent, "Powered by BabyBearsEngine");
        Assert.AreEqual(1, bannerCount, "Banner preamble should appear exactly once per run.");
    }

    // ─── Initialise guards ───

    [TestMethod]
    public void Initialise_NullLogSettings_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Logger.Initialise(null!, ConsoleSettings.Default));
    }

    [TestMethod]
    public void Initialise_NullConsoleSettings_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Logger.Initialise(LogSettings.Default, null!));
    }

    [TestMethod]
    public void Initialise_ResetsDedupeSet()
    {
        Logger.Initialise(TestSettings(dedupeGLErrors: true), PlainConsole());
        Logger.GLError("first");

        // Re-initialise — should clear the dedupe set so the next call from the same line logs again.
        Logger.Initialise(TestSettings(dedupeGLErrors: true), PlainConsole());
        Logger.GLError("first");

        int count = CountOccurrences(ConsoleOutput, "first");
        Assert.AreEqual(2, count, "Re-initialise should clear the dedupe set.");
    }

    // ─── LogFileMode ───

    [TestMethod]
    public void LogFileMode_AppendToExisting_PreservesExistingContent()
    {
        File.WriteAllText(_logPath, "pre-existing\n");

        Logger.Initialise(TestSettings(fileMode: LogFileMode.AppendToExisting), PlainConsole());
        Logger.Information("new");

        StringAssert.Contains(LogFileContent, "pre-existing");
        StringAssert.Contains(LogFileContent, "new");
    }

    [TestMethod]
    public void LogFileMode_OverwriteExisting_RemovesExistingContent()
    {
        File.WriteAllText(_logPath, "pre-existing\n");

        Logger.Initialise(TestSettings(fileMode: LogFileMode.OverwriteExisting), PlainConsole());
        Logger.Information("new");

        Assert.IsFalse(LogFileContent.Contains("pre-existing"));
        StringAssert.Contains(LogFileContent, "new");
    }

    [TestMethod]
    public void LogFileMode_NewFilePerRun_CreatesTimestampedFile_OriginalUntouched()
    {
        File.WriteAllText(_logPath, "pre-existing\n");

        Logger.Initialise(TestSettings(fileMode: LogFileMode.NewFilePerRun), PlainConsole());
        Logger.Information("new");

        string originalContent = File.ReadAllText(_logPath);
        StringAssert.Contains(originalContent, "pre-existing");
        Assert.IsFalse(originalContent.Contains("new"), "Original file should be untouched.");

        var siblings = Directory.GetFiles(_tempDir, "log_*.log");
        Assert.AreEqual(1, siblings.Length, "Expected exactly one timestamped sibling file.");
        StringAssert.Contains(File.ReadAllText(siblings[0]), "new");
    }

    // ─── Helpers ───

    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0;
        int idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) != -1)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }
}
