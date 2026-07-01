using BabyBearsEngine.Input;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ClipboardFacadeTests
{
    private sealed class FakeClipboard : IClipboard
    {
        public List<(string Method, object? Arg)> Calls { get; } = [];
        public string TextToReturn { get; set; } = string.Empty;

        public string GetText()
        {
            Calls.Add((nameof(GetText), null));
            return TextToReturn;
        }

        public void SetText(string text) => Calls.Add((nameof(SetText), text));
    }

    private FakeClipboard _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeClipboard();
        EngineConfiguration.ClipboardService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    // Service-not-installed contract

    [TestMethod]
    public void GetText_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Clipboard.GetText());
    }

    [TestMethod]
    public void SetText_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Clipboard.SetText("text"));
    }

    // Routing

    [TestMethod]
    public void GetText_ReturnsServiceValue()
    {
        _fake.TextToReturn = "hello";

        string result = Clipboard.GetText();

        Assert.AreEqual("hello", result);
        Assert.AreEqual((nameof(IClipboard.GetText), (object?)null), _fake.Calls.Single());
    }

    [TestMethod]
    public void SetText_PassesTextToService()
    {
        Clipboard.SetText("world");

        Assert.AreEqual((nameof(IClipboard.SetText), (object?)"world"), _fake.Calls.Single());
    }

    // Service substitution after install

    [TestMethod]
    public void ReplacingService_RoutesToNewInstance()
    {
        var second = new FakeClipboard { TextToReturn = "second" };
        EngineConfiguration.ClipboardService = second;

        string result = Clipboard.GetText();

        Assert.AreEqual("second", result);
        Assert.IsEmpty(_fake.Calls);
        Assert.HasCount(1, second.Calls);
    }
}
