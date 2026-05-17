using System.Linq;
using System.Text.Json;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class JsonTests
{
    private sealed record SimpleRecord(string Name, int Value);

    // Serialize / Deserialize round-trip

    [TestMethod]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        SimpleRecord original = new("hello", 42);
        string json = Json.Serialize(original);
        SimpleRecord result = Json.Deserialize<SimpleRecord>(json);

        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void Serialize_ProducesIndentedJson()
    {
        SimpleRecord record = new("x", 1);
        string json = Json.Serialize(record);

        Assert.IsTrue(json.Contains('\n'));
    }

    // Deserialize — invalid input throws

    [TestMethod]
    public void Deserialize_MalformedJson_Throws()
    {
        Assert.ThrowsExactly<JsonException>(() => Json.Deserialize<SimpleRecord>("not json"));
    }

    // TryDeserialize

    [TestMethod]
    public void TryDeserialize_ValidJson_ReturnsValue()
    {
        SimpleRecord original = new("hello", 42);
        string json = Json.Serialize(original);

        SimpleRecord? result = Json.TryDeserialize<SimpleRecord>(json);

        Assert.IsNotNull(result);
        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void TryDeserialize_MalformedJson_ReturnsDefault()
    {
        SimpleRecord? result = Json.TryDeserialize<SimpleRecord>("not json");

        Assert.IsNull(result);
    }

    // DefaultOptions contains BBE converters

    [TestMethod]
    public void DefaultOptions_IncludesColourJsonConverter()
    {
        Assert.IsTrue(Json.DefaultOptions.Converters.Any(c => c is ColourJsonConverter));
    }

    [TestMethod]
    public void DefaultOptions_IncludesPointJsonConverter()
    {
        Assert.IsTrue(Json.DefaultOptions.Converters.Any(c => c is PointJsonConverter));
    }

    [TestMethod]
    public void DefaultOptions_IncludesRectJsonConverter()
    {
        Assert.IsTrue(Json.DefaultOptions.Converters.Any(c => c is RectJsonConverter));
    }

    [TestMethod]
    public void DefaultOptions_IncludesTwoDimensionalArrayConverter()
    {
        Assert.IsTrue(Json.DefaultOptions.Converters.Any(c => c is TwoDimensionalArrayConverter));
    }
}
