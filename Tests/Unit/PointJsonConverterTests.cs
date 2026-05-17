using BabyBearsEngine.Geometry;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PointJsonConverterTests
{
    [TestMethod]
    public void Serialize_WritesXAndY()
    {
        Point point = new(1.5f, 2.5f);
        string json = Json.Serialize(point);

        Assert.IsTrue(json.Contains("\"X\""));
        Assert.IsTrue(json.Contains("\"Y\""));
    }

    [TestMethod]
    public void Deserialize_ReadsXAndY()
    {
        Point result = Json.Deserialize<Point>("{\"X\":1.5,\"Y\":2.5}");

        Assert.AreEqual(1.5f, result.X, delta: 1e-5f);
        Assert.AreEqual(2.5f, result.Y, delta: 1e-5f);
    }

    [TestMethod]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        Point original = new(3.14f, -2.71f);
        string json = Json.Serialize(original);
        Point result = Json.Deserialize<Point>(json);

        Assert.AreEqual(original.X, result.X, delta: 1e-5f);
        Assert.AreEqual(original.Y, result.Y, delta: 1e-5f);
    }

    [TestMethod]
    public void Deserialize_UnknownProperty_IsIgnored()
    {
        Point result = Json.Deserialize<Point>("{\"X\":1.0,\"Y\":2.0,\"Z\":3.0}");

        Assert.AreEqual(1.0f, result.X, delta: 1e-5f);
        Assert.AreEqual(2.0f, result.Y, delta: 1e-5f);
    }
}
