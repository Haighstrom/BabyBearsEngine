using System;
using System.IO;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TrueTypeFontFileResolverTests
{
    private string _originalDirectory = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        // The resolver probes relative paths (Assets/Fonts/...) against the working directory.
        // Anchor it to the test output directory, where the engine's shipped fonts are copied,
        // so the test is independent of whatever working directory the runner happens to use.
        _originalDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = AppContext.BaseDirectory;
    }

    [TestCleanup]
    public void Cleanup() => Environment.CurrentDirectory = _originalDirectory;

    [TestMethod]
    public void Resolve_ShippedFont_ReturnsExistingPath()
    {
        string path = TrueTypeFontFileResolver.Resolve("Arial");

        Assert.IsTrue(File.Exists(path), $"Resolved path should exist: {path}");
    }

    [TestMethod]
    public void Resolve_FontNameWithSpaces_FindsTtf()
    {
        string path = TrueTypeFontFileResolver.Resolve("Times New Roman");

        Assert.IsTrue(path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase), $"Expected a .ttf path, got: {path}");
        Assert.IsTrue(File.Exists(path), $"Resolved path should exist: {path}");
    }

    [TestMethod]
    public void Resolve_MissingFont_ThrowsFileNotFound()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => TrueTypeFontFileResolver.Resolve("ThisFontDoesNotExist_xyz"));
    }
}
