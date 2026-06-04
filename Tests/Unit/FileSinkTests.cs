using System;
using System.IO;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FileSinkTests
{
    private string _tempDir = null!;
    private string _filePath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "bbe_filesink_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "out.log");
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

    // FileSink now holds a FileAccess.Write handle for the sink's lifetime, so a reader needs
    // FileShare.ReadWrite to coexist (matches what Notepad++ / tail / etc. do externally).
    // File.ReadAllText defaults to FileShare.Read, which conflicts and throws IOException.
    private string ReadFile()
    {
        using FileStream stream = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    [TestMethod]
    public void Write_NoPreamble_AppendsMessageWithTrailingNewline()
    {
        var sink = new FileSink(_filePath);

        sink.Write(LogLevel.Information, "hello", exception: null);

        Assert.AreEqual("hello" + Environment.NewLine, ReadFile());
    }

    [TestMethod]
    public void Write_WithExceptionButNoPreamble_AppendsMessageThenException()
    {
        var sink = new FileSink(_filePath);
        var ex = new InvalidOperationException("boom");

        sink.Write(LogLevel.Error, "failed", ex);

        string content = ReadFile();
        Assert.Contains("failed", content);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("boom", content);
    }

    [TestMethod]
    public void Write_FirstCallWithPreamble_PreambleAppearsBeforeMessage()
    {
        var sink = new FileSink(_filePath, "PREAMBLE\n");

        sink.Write(LogLevel.Information, "first", exception: null);

        string content = ReadFile();
        int preambleIdx = content.IndexOf("PREAMBLE", StringComparison.Ordinal);
        int messageIdx = content.IndexOf("first", StringComparison.Ordinal);

        Assert.AreNotEqual(-1, preambleIdx, "Preamble should be written.");
        Assert.AreNotEqual(-1, messageIdx, "Message should be written.");
        Assert.IsLessThan(messageIdx, preambleIdx, "Preamble must appear before the first log message.");
    }

    [TestMethod]
    public void Write_PreambleEmittedExactlyOnce_AcrossMultipleWrites()
    {
        var sink = new FileSink(_filePath, "PREAMBLE\n");

        sink.Write(LogLevel.Information, "one", exception: null);
        sink.Write(LogLevel.Information, "two", exception: null);
        sink.Write(LogLevel.Information, "three", exception: null);

        string content = ReadFile();
        int preambleOccurrences = CountOccurrences(content, "PREAMBLE");

        Assert.AreEqual(1, preambleOccurrences, "Preamble must appear exactly once across multiple writes.");
        Assert.Contains("one", content);
        Assert.Contains("two", content);
        Assert.Contains("three", content);
    }

    [TestMethod]
    public void Write_NoCalls_FileNotCreated()
    {
        _ = new FileSink(_filePath, "PREAMBLE\n");

        Assert.IsFalse(File.Exists(_filePath), "FileSink construction alone must not create the file.");
    }

    [TestMethod]
    public void Write_ContentVisibleImmediately_BeforeSinkDisposal()
    {
        // The held-StreamWriter implementation must AutoFlush so each line lands on disk before
        // the sink is disposed — otherwise external tools tailing the log see nothing until the
        // process exits, defeating the point of crash-safety.
        using var sink = new FileSink(_filePath);

        sink.Write(LogLevel.Information, "midflight", exception: null);

        Assert.Contains("midflight", ReadFile());
    }

    [TestMethod]
    public void Dispose_FlushesAndReleasesHandle()
    {
        // After Dispose, an external process must be able to take an exclusive lock on the file —
        // the held StreamWriter must have released its handle.
        var sink = new FileSink(_filePath);
        sink.Write(LogLevel.Information, "before-dispose", exception: null);

        sink.Dispose();

        // Opening exclusively should succeed only if the sink let go of the handle.
        using FileStream exclusive = new(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.IsGreaterThan(0, exclusive.Length);
    }

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
