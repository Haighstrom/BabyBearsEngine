using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ColourJsonConverterTests
{
    // Write — always hex

    [TestMethod]
    public void Serialize_WritesHexString()
    {
        Colour colour = new(255, 0, 0);
        string json = Json.Serialize(colour);

        Assert.AreEqual("\"#FF0000FF\"", json);
    }

    [TestMethod]
    public void Serialize_WithAlpha_IncludesAlphaInHex()
    {
        Colour colour = new(0, 128, 255, 64);
        string json = Json.Serialize(colour);

        Assert.AreEqual("\"#0080FF40\"", json);
    }

    // Read — hex string

    [TestMethod]
    public void Deserialize_HexString_ParsesCorrectly()
    {
        Colour result = Json.Deserialize<Colour>("\"#FF4400FF\"");

        Assert.AreEqual(255, result.R);
        Assert.AreEqual(68, result.G);
        Assert.AreEqual(0, result.B);
        Assert.AreEqual(255, result.A);
    }

    [TestMethod]
    public void Deserialize_HexStringShortForm_DefaultsAlphaTo255()
    {
        Colour result = Json.Deserialize<Colour>("\"#FF4400\"");

        Assert.AreEqual(255, result.R);
        Assert.AreEqual(68, result.G);
        Assert.AreEqual(0, result.B);
        Assert.AreEqual(255, result.A);
    }

    // Read — object form

    [TestMethod]
    public void Deserialize_ObjectForm_ParsesAllComponents()
    {
        Colour result = Json.Deserialize<Colour>("{\"R\":255,\"G\":68,\"B\":0,\"A\":128}");

        Assert.AreEqual(255, result.R);
        Assert.AreEqual(68, result.G);
        Assert.AreEqual(0, result.B);
        Assert.AreEqual(128, result.A);
    }

    [TestMethod]
    public void Deserialize_ObjectFormMissingA_DefaultsAlphaTo255()
    {
        Colour result = Json.Deserialize<Colour>("{\"R\":10,\"G\":20,\"B\":30}");

        Assert.AreEqual(10, result.R);
        Assert.AreEqual(20, result.G);
        Assert.AreEqual(30, result.B);
        Assert.AreEqual(255, result.A);
    }

    // Round-trip

    [TestMethod]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        Colour original = new(12, 34, 56, 78);
        string json = Json.Serialize(original);
        Colour result = Json.Deserialize<Colour>(json);

        Assert.AreEqual(original, result);
    }
}
