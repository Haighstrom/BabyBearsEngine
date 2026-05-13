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

    private string ReadFile() => File.ReadAllText(_filePath);

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
        StringAssert.Contains(content, "failed");
        StringAssert.Contains(content, "InvalidOperationException");
        StringAssert.Contains(content, "boom");
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
        Assert.IsTrue(preambleIdx < messageIdx, "Preamble must appear before the first log message.");
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
        StringAssert.Contains(content, "one");
        StringAssert.Contains(content, "two");
        StringAssert.Contains(content, "three");
    }

    [TestMethod]
    public void Write_NoCalls_FileNotCreated()
    {
        _ = new FileSink(_filePath, "PREAMBLE\n");

        Assert.IsFalse(File.Exists(_filePath), "FileSink construction alone must not create the file.");
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
