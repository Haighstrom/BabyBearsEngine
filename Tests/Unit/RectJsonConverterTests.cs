using BabyBearsEngine.Geometry;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RectJsonConverterTests
{
    [TestMethod]
    public void Serialize_WritesXYWH()
    {
        Rect rect = new(1f, 2f, 100f, 50f);
        string json = Json.Serialize(rect);

        Assert.IsTrue(json.Contains("\"X\""));
        Assert.IsTrue(json.Contains("\"Y\""));
        Assert.IsTrue(json.Contains("\"W\""));
        Assert.IsTrue(json.Contains("\"H\""));
    }

    [TestMethod]
    public void Serialize_DoesNotWriteComputedProperties()
    {
        Rect rect = new(1f, 2f, 100f, 50f);
        string json = Json.Serialize(rect);

        Assert.IsFalse(json.Contains("\"Left\""));
        Assert.IsFalse(json.Contains("\"Right\""));
        Assert.IsFalse(json.Contains("\"Top\""));
        Assert.IsFalse(json.Contains("\"Bottom\""));
    }

    [TestMethod]
    public void Deserialize_ReadsXYWH()
    {
        Rect result = Json.Deserialize<Rect>("{\"X\":1,\"Y\":2,\"W\":100,\"H\":50}");

        Assert.AreEqual(1f, result.X, delta: 1e-5f);
        Assert.AreEqual(2f, result.Y, delta: 1e-5f);
        Assert.AreEqual(100f, result.W, delta: 1e-5f);
        Assert.AreEqual(50f, result.H, delta: 1e-5f);
    }

    [TestMethod]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        Rect original = new(5f, 10f, 200f, 100f);
        string json = Json.Serialize(original);
        Rect result = Json.Deserialize<Rect>(json);

        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void Deserialize_UnknownProperty_IsIgnored()
    {
        Rect result = Json.Deserialize<Rect>("{\"X\":1,\"Y\":2,\"W\":3,\"H\":4,\"Extra\":99}");

        Assert.AreEqual(1f, result.X, delta: 1e-5f);
        Assert.AreEqual(4f, result.H, delta: 1e-5f);
    }
}
