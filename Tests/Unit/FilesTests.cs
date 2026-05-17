using System;
using System.IO;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FilesTests
{
    private string _tempDir = "";

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string TempFile(string name = "test.txt") => Path.Combine(_tempDir, name);

    // Text read / write

    [TestMethod]
    public void WriteText_ThenReadText_RoundTrips()
    {
        Files.WriteText(TempFile(), "hello world");

        Assert.AreEqual("hello world", Files.ReadText(TempFile()));
    }

    [TestMethod]
    public void WriteLines_ThenReadLines_RoundTrips()
    {
        Files.WriteLines(TempFile(), ["alpha", "beta", "gamma"]);

        string[] lines = Files.ReadLines(TempFile());

        Assert.HasCount(3, lines);
        Assert.AreEqual("alpha", lines[0]);
        Assert.AreEqual("gamma", lines[2]);
    }

    [TestMethod]
    public void AppendText_AddsToExistingContent()
    {
        Files.WriteText(TempFile(), "hello");
        Files.AppendText(TempFile(), " world");

        Assert.AreEqual("hello world", Files.ReadText(TempFile()));
    }

    [TestMethod]
    public void AppendLines_AddsLinesToExistingFile()
    {
        Files.WriteLines(TempFile(), ["line1"]);
        Files.AppendLines(TempFile(), ["line2"]);

        string[] lines = Files.ReadLines(TempFile());

        Assert.HasCount(2, lines);
        Assert.AreEqual("line2", lines[1]);
    }

    [TestMethod]
    public void TryReadText_FileExists_ReturnsContent()
    {
        Files.WriteText(TempFile(), "content");

        Assert.AreEqual("content", Files.TryReadText(TempFile()));
    }

    [TestMethod]
    public void TryReadText_FileMissing_ReturnsNull()
    {
        Assert.IsNull(Files.TryReadText(TempFile("nonexistent.txt")));
    }

    // JSON file read / write

    private sealed record JsonPayload(string Name, int Value);

    [TestMethod]
    public void WriteJsonFile_ThenReadJsonFile_RoundTrips()
    {
        JsonPayload original = new("hello", 42);
        string path = TempFile("data.json");

        Files.WriteJsonFile(path, original);
        JsonPayload result = Files.ReadJsonFile<JsonPayload>(path);

        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void TryReadJsonFile_FileMissing_ReturnsNull()
    {
        Assert.IsNull(Files.TryReadJsonFile<JsonPayload>(TempFile("missing.json")));
    }

    [TestMethod]
    public void TryReadJsonFile_MalformedContent_ReturnsNull()
    {
        string path = TempFile("bad.json");
        Files.WriteText(path, "not json");

        Assert.IsNull(Files.TryReadJsonFile<JsonPayload>(path));
    }

    // XML file read / write

    public sealed class XmlPayload
    {
        public string Name { get; set; } = "";
        public int Value { get; set; } = 0;
    }

    [TestMethod]
    public void WriteXmlFile_ThenReadXmlFile_RoundTrips()
    {
        XmlPayload original = new() { Name = "hello", Value = 42 };
        string path = TempFile("data.xml");

        Files.WriteXmlFile(path, original);
        XmlPayload result = Files.ReadXmlFile<XmlPayload>(path);

        Assert.AreEqual(original.Name, result.Name);
        Assert.AreEqual(original.Value, result.Value);
    }

    [TestMethod]
    public void TryReadXmlFile_FileMissing_ReturnsNull()
    {
        Assert.IsNull(Files.TryReadXmlFile<XmlPayload>(TempFile("missing.xml")));
    }

    // CSV file read / write

    [TestMethod]
    public void WriteCsvFile_ThenReadCsvFile_RoundTrips()
    {
        int[,] original = { { 1, 2, 3 }, { 4, 5, 6 } };
        string path = TempFile("data.csv");

        Files.WriteCsvFile(path, original);
        int[,] result = Files.ReadCsvFile<int>(path);

        Assert.AreEqual(original.GetLength(0), result.GetLength(0));
        Assert.AreEqual(original.GetLength(1), result.GetLength(1));
        Assert.AreEqual(1, result[0, 0]);
        Assert.AreEqual(6, result[1, 2]);
    }

    [TestMethod]
    public void WriteCsvFile_CustomSeparator_RoundTrips()
    {
        float[,] original = { { 1.5f, 2.5f } };
        string path = TempFile("data.csv");

        Files.WriteCsvFile(path, original, ';');
        float[,] result = Files.ReadCsvFile<float>(path, ';');

        Assert.AreEqual(1.5f, result[0, 0], delta: 1e-5f);
        Assert.AreEqual(2.5f, result[0, 1], delta: 1e-5f);
    }

    // GetFiles / GetDirectories

    [TestMethod]
    public void GetFiles_ReturnsFilesInDirectory()
    {
        Files.WriteText(Path.Combine(_tempDir, "a.txt"), "");
        Files.WriteText(Path.Combine(_tempDir, "b.txt"), "");

        var files = Files.GetFiles(_tempDir, includeSubDirectories: false);

        Assert.HasCount(2, files);
    }

    [TestMethod]
    public void GetFiles_WithSearchPattern_FiltersResults()
    {
        Files.WriteText(Path.Combine(_tempDir, "a.txt"), "");
        Files.WriteText(Path.Combine(_tempDir, "b.json"), "");

        var files = Files.GetFiles(_tempDir, includeSubDirectories: false, searchPattern: "*.txt");

        Assert.HasCount(1, files);
        Assert.IsTrue(files[0].EndsWith("a.txt"));
    }

    // Path helpers

    [TestMethod]
    public void AppDataDirectory_ReturnsNonEmptyPath()
    {
        string path = Files.AppDataDirectory("TestApp");

        Assert.IsTrue(path.Length > 0);
        Assert.IsTrue(path.EndsWith("TestApp"));
    }

    [TestMethod]
    public void ExecutingDirectory_ReturnsNonEmptyPath()
    {
        string path = Files.ExecutingDirectory();

        Assert.IsTrue(path.Length > 0);
        Assert.IsTrue(Directory.Exists(path));
    }

    // IoSettings default

    [TestMethod]
    public void Settings_Default_HasPositiveRetryCount()
    {
        Assert.IsGreaterThan(0, Files.Settings.RetryCount);
    }
}
