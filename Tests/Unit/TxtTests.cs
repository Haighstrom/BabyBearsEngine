using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TxtTests
{
    [TestMethod]
    public void Serialize_JoinsLinesWithLineFeed()
    {
        string result = Txt.Serialize(["one", "two", "three"]);

        Assert.AreEqual("one\ntwo\nthree", result);
    }

    [TestMethod]
    public void Serialize_EmptyEnumerable_ReturnsEmptyString()
    {
        string result = Txt.Serialize([]);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void Serialize_SingleLine_ReturnsLineUnchanged()
    {
        string result = Txt.Serialize(["only"]);

        Assert.AreEqual("only", result);
    }

    [TestMethod]
    public void Deserialize_LfSeparatedText_SplitsIntoLines()
    {
        string[] result = Txt.Deserialize("a\nb\nc");

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
    }

    [TestMethod]
    public void Deserialize_CrlfSeparatedText_SplitsIntoLines()
    {
        string[] result = Txt.Deserialize("a\r\nb\r\nc");

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
    }

    [TestMethod]
    public void Deserialize_CrSeparatedText_SplitsIntoLines()
    {
        string[] result = Txt.Deserialize("a\rb\rc");

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
    }

    [TestMethod]
    public void Deserialize_EmptyString_ReturnsEmptyArray()
    {
        string[] result = Txt.Deserialize(string.Empty);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void Deserialize_NoLineBreaks_ReturnsSingleElement()
    {
        string[] result = Txt.Deserialize("just one line");

        Assert.HasCount(1, result);
        Assert.AreEqual("just one line", result[0]);
    }

    [TestMethod]
    public void RoundTrip_PreservesLines()
    {
        string[] original = ["first", "second", "", "fourth"];

        string text = Txt.Serialize(original);
        string[] result = Txt.Deserialize(text);

        CollectionAssert.AreEqual(original, result);
    }
}
