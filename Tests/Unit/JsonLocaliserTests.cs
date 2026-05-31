using System;
using System.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class JsonLocaliserTests
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

    private void WriteLocale(string locale, params (string Key, string Value)[] entries)
    {
        string json = "{" + string.Join(",", entries.Select(e => $"\"{e.Key}\":\"{e.Value}\"")) + "}";
        File.WriteAllText(Path.Combine(_tempDir, $"{locale}.json"), json);
    }

    [TestMethod]
    public void Constructor_LoadsAllJsonFiles()
    {
        WriteLocale("en", ("menu.play", "Play"));
        WriteLocale("fr", ("menu.play", "Jouer"));

        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");

        Assert.Contains("en", localiser.AvailableLocales);
        Assert.Contains("fr", localiser.AvailableLocales);
        Assert.HasCount(2, localiser.AvailableLocales);
    }

    [TestMethod]
    public void Constructor_DefaultLocaleSetsCurrent()
    {
        WriteLocale("en", ("menu.play", "Play"));
        WriteLocale("fr", ("menu.play", "Jouer"));

        JsonLocaliser localiser = new(_tempDir, defaultLocale: "fr", fallbackLocale: "en");

        Assert.AreEqual("fr", localiser.CurrentLocale);
    }

    [TestMethod]
    public void Constructor_MissingDefaultLocale_Throws()
    {
        WriteLocale("en", ("menu.play", "Play"));

        Assert.ThrowsExactly<ArgumentException>(() =>
            new JsonLocaliser(_tempDir, defaultLocale: "fr", fallbackLocale: "en"));
    }

    [TestMethod]
    public void Constructor_MissingFolder_Throws()
    {
        string missing = Path.Combine(_tempDir, "nope");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() =>
            new JsonLocaliser(missing, defaultLocale: "en", fallbackLocale: "en"));
    }

    [TestMethod]
    public void Get_ReturnsCurrentLocaleValue()
    {
        WriteLocale("en", ("menu.play", "Play"));
        WriteLocale("fr", ("menu.play", "Jouer"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "fr", fallbackLocale: "en");

        Assert.AreEqual("Jouer", localiser.Get("menu.play"));
    }

    [TestMethod]
    public void Get_MissingInCurrent_FallsBackToFallback()
    {
        WriteLocale("en", ("menu.play", "Play"), ("menu.quit", "Quit"));
        WriteLocale("fr", ("menu.play", "Jouer")); // no menu.quit translation
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "fr", fallbackLocale: "en");

        Assert.AreEqual("Quit", localiser.Get("menu.quit"));
    }

    [TestMethod]
    public void Get_MissingInBoth_ReturnsKey()
    {
        WriteLocale("en", ("menu.play", "Play"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");

        Assert.AreEqual("menu.missing", localiser.Get("menu.missing"));
    }

    [TestMethod]
    public void Format_SubstitutesArguments()
    {
        WriteLocale("en", ("shop.coins", "You have {0} coins"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");

        Assert.AreEqual("You have 7 coins", localiser.Format("shop.coins", 7));
    }

    [TestMethod]
    public void Format_NoArgs_ReturnsRawString()
    {
        WriteLocale("en", ("menu.play", "Play"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");

        Assert.AreEqual("Play", localiser.Format("menu.play"));
    }

    [TestMethod]
    public void SetLocale_SwitchesCurrentAndFiresEvent()
    {
        WriteLocale("en", ("menu.play", "Play"));
        WriteLocale("fr", ("menu.play", "Jouer"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");
        LocaleChangedEventArgs? received = null;
        localiser.LocaleChanged += args => received = args;

        localiser.SetLocale("fr");

        Assert.AreEqual("fr", localiser.CurrentLocale);
        Assert.IsNotNull(received);
        Assert.AreEqual("en", received.OldLocale);
        Assert.AreEqual("fr", received.NewLocale);
    }

    [TestMethod]
    public void SetLocale_SameAsCurrent_DoesNotFireEvent()
    {
        WriteLocale("en", ("menu.play", "Play"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");
        bool fired = false;
        localiser.LocaleChanged += _ => fired = true;

        localiser.SetLocale("en");

        Assert.IsFalse(fired);
    }

    [TestMethod]
    public void SetLocale_Unknown_Throws()
    {
        WriteLocale("en", ("menu.play", "Play"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");

        Assert.ThrowsExactly<ArgumentException>(() => localiser.SetLocale("de"));
    }

    [TestMethod]
    public void SetLocale_CaseInsensitive_MatchesFileName()
    {
        WriteLocale("en", ("menu.play", "Play"));
        WriteLocale("fr", ("menu.play", "Jouer"));
        JsonLocaliser localiser = new(_tempDir, defaultLocale: "en", fallbackLocale: "en");

        localiser.SetLocale("FR");

        Assert.AreEqual("Jouer", localiser.Get("menu.play"));
    }
}
