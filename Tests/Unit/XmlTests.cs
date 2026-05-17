using System;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class XmlTests
{
    public sealed class SimpleData
    {
        public string Name { get; set; } = "";
        public int Value { get; set; } = 0;
    }

    // Serialize / Deserialize round-trip

    [TestMethod]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        SimpleData original = new() { Name = "hello", Value = 42 };
        string xml = Xml.Serialize(original);
        SimpleData result = Xml.Deserialize<SimpleData>(xml);

        Assert.AreEqual(original.Name, result.Name);
        Assert.AreEqual(original.Value, result.Value);
    }

    [TestMethod]
    public void Serialize_ProducesXmlContent()
    {
        SimpleData data = new() { Name = "test", Value = 1 };
        string xml = Xml.Serialize(data);

        Assert.IsTrue(xml.Contains("<?xml"));
        Assert.IsTrue(xml.Contains("test"));
    }

    // Deserialize — invalid input throws

    [TestMethod]
    public void Deserialize_MalformedXml_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Xml.Deserialize<SimpleData>("not xml"));
    }

    // TryDeserialize

    [TestMethod]
    public void TryDeserialize_ValidXml_ReturnsValue()
    {
        SimpleData original = new() { Name = "hello", Value = 42 };
        string xml = Xml.Serialize(original);

        SimpleData? result = Xml.TryDeserialize<SimpleData>(xml);

        Assert.IsNotNull(result);
        Assert.AreEqual(original.Name, result.Name);
        Assert.AreEqual(original.Value, result.Value);
    }

    [TestMethod]
    public void TryDeserialize_MalformedXml_ReturnsNull()
    {
        SimpleData? result = Xml.TryDeserialize<SimpleData>("not xml");

        Assert.IsNull(result);
    }
}
