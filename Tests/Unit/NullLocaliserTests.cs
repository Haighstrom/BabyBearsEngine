using System;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class NullLocaliserTests
{
    private NullLocaliser _localiser = null!;

    [TestInitialize]
    public void Setup() => _localiser = new NullLocaliser();

    [TestMethod]
    public void CurrentLocale_IsEn()
    {
        Assert.AreEqual("en", _localiser.CurrentLocale);
    }

    [TestMethod]
    public void AvailableLocales_IsEmpty()
    {
        Assert.IsEmpty(_localiser.AvailableLocales);
    }

    [TestMethod]
    public void Get_ReturnsKeyVerbatim()
    {
        Assert.AreEqual("menu.play", _localiser.Get("menu.play"));
    }

    [TestMethod]
    public void Get_NullKey_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _localiser.Get(null!));
    }

    [TestMethod]
    public void Format_NoArgs_ReturnsKey()
    {
        Assert.AreEqual("menu.play", _localiser.Format("menu.play"));
    }

    [TestMethod]
    public void Format_WithArgs_RunsStringFormatOnKey()
    {
        // Falls back to string.Format treating the key as a template — useful only if the key
        // itself contains placeholders, but predictable behaviour either way.
        Assert.AreEqual("count=3", _localiser.Format("count={0}", 3));
    }

    [TestMethod]
    public void SetLocale_SameAsCurrent_DoesNotThrow()
    {
        _localiser.SetLocale("en");
    }

    [TestMethod]
    public void SetLocale_Different_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _localiser.SetLocale("fr"));
    }

    [TestMethod]
    public void SetLocale_Null_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _localiser.SetLocale(null!));
    }
}
