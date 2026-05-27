using System;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CsvTests
{
    // Serialize / Deserialize round-trip — integers

    [TestMethod]
    public void Serialize_IntArray_ProducesExpectedCsv()
    {
        int[,] data = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        string result = Csv.Serialize(data);

        Assert.AreEqual("1,2,3\n4,5,6", result);
    }

    [TestMethod]
    public void Deserialize_IntCsv_ProducesExpectedArray()
    {
        int[,] result = Csv.Deserialize<int>("1,2,3\n4,5,6");

        Assert.AreEqual(2, result.GetLength(0));
        Assert.AreEqual(3, result.GetLength(1));
        Assert.AreEqual(1, result[0, 0]);
        Assert.AreEqual(6, result[1, 2]);
    }

    [TestMethod]
    public void RoundTrip_IntArray_PreservesValues()
    {
        int[,] original = new[,] { { 10, 20 }, { 30, 40 }, { 50, 60 } };

        string csv = Csv.Serialize(original);
        int[,] result = Csv.Deserialize<int>(csv);

        Assert.AreEqual(original.GetLength(0), result.GetLength(0));
        Assert.AreEqual(original.GetLength(1), result.GetLength(1));
        for (int row = 0; row < original.GetLength(0); row++)
        {
            for (int col = 0; col < original.GetLength(1); col++)
            {
                Assert.AreEqual(original[row, col], result[row, col]);
            }
        }
    }

    // Separator

    [TestMethod]
    public void Serialize_WithSemicolonSeparator_UsesSeparator()
    {
        int[,] data = new[,] { { 1, 2 } };

        string result = Csv.Serialize(data, separator: ';');

        Assert.AreEqual("1;2", result);
    }

    [TestMethod]
    public void Deserialize_WithSemicolonSeparator_ParsesCorrectly()
    {
        int[,] result = Csv.Deserialize<int>("1;2;3", separator: ';');

        Assert.AreEqual(3, result.GetLength(1));
        Assert.AreEqual(2, result[0, 1]);
    }

    // Escaping — RFC 4180

    [TestMethod]
    public void Serialize_FieldWithSeparator_IsQuoted()
    {
        string[,] data = new[,] { { "a,b", "c" } };

        string result = Csv.Serialize(data);

        Assert.AreEqual("\"a,b\",c", result);
    }

    [TestMethod]
    public void Serialize_FieldWithDoubleQuote_IsQuotedAndEscaped()
    {
        string[,] data = new[,] { { "she said \"hi\"" } };

        string result = Csv.Serialize(data);

        Assert.AreEqual("\"she said \"\"hi\"\"\"", result);
    }

    [TestMethod]
    public void Serialize_FieldWithNewline_IsQuoted()
    {
        string[,] data = new[,] { { "line1\nline2" } };

        string result = Csv.Serialize(data);

        Assert.AreEqual("\"line1\nline2\"", result);
    }

    [TestMethod]
    public void Deserialize_QuotedFieldWithSeparator_ParsesAsSingleField()
    {
        string[,] result = Csv.Deserialize<string>("\"a,b\",c");

        Assert.AreEqual(1, result.GetLength(0));
        Assert.AreEqual(2, result.GetLength(1));
        Assert.AreEqual("a,b", result[0, 0]);
        Assert.AreEqual("c", result[0, 1]);
    }

    [TestMethod]
    public void Deserialize_QuotedFieldWithEscapedQuote_ParsesCorrectly()
    {
        string[,] result = Csv.Deserialize<string>("\"she said \"\"hi\"\"\"");

        Assert.AreEqual("she said \"hi\"", result[0, 0]);
    }

    [TestMethod]
    public void Deserialize_QuotedFieldWithEmbeddedNewline_ParsesAsSingleRow()
    {
        string[,] result = Csv.Deserialize<string>("\"line1\nline2\",x");

        Assert.AreEqual(1, result.GetLength(0));
        Assert.AreEqual(2, result.GetLength(1));
        Assert.AreEqual("line1\nline2", result[0, 0]);
        Assert.AreEqual("x", result[0, 1]);
    }

    [TestMethod]
    public void RoundTrip_StringWithSpecialChars_Preserved()
    {
        string[,] original = new[,]
        {
            { "plain", "with,comma", "with\"quote" },
            { "with\nnewline", "normal", "tab\there" },
        };

        string csv = Csv.Serialize(original);
        string[,] result = Csv.Deserialize<string>(csv);

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Assert.AreEqual(original[row, col], result[row, col]);
            }
        }
    }

    // Line endings

    [TestMethod]
    public void Deserialize_CrlfLineEndings_ParsesRows()
    {
        int[,] result = Csv.Deserialize<int>("1,2\r\n3,4");

        Assert.AreEqual(2, result.GetLength(0));
        Assert.AreEqual(3, result[1, 0]);
        Assert.AreEqual(4, result[1, 1]);
    }

    [TestMethod]
    public void Deserialize_LfLineEndings_ParsesRows()
    {
        int[,] result = Csv.Deserialize<int>("1,2\n3,4");

        Assert.AreEqual(2, result.GetLength(0));
        Assert.AreEqual(3, result[1, 0]);
    }

    [TestMethod]
    public void Deserialize_TrailingNewline_DoesNotProduceEmptyRow()
    {
        int[,] result = Csv.Deserialize<int>("1,2\n3,4\n");

        Assert.AreEqual(2, result.GetLength(0));
    }

    // Edge cases

    [TestMethod]
    public void Deserialize_EmptyString_ReturnsEmptyArray()
    {
        int[,] result = Csv.Deserialize<int>("");

        Assert.AreEqual(0, result.GetLength(0));
        Assert.AreEqual(0, result.GetLength(1));
    }

    [TestMethod]
    public void Deserialize_RaggedRows_PadsWithDefault()
    {
        int[,] result = Csv.Deserialize<int>("1,2,3\n4,5");

        Assert.AreEqual(2, result.GetLength(0));
        Assert.AreEqual(3, result.GetLength(1));
        Assert.AreEqual(0, result[1, 2]);
    }

    // Header support

    [TestMethod]
    public void SerializeWithHeader_PrependsHeaderRow()
    {
        int[,] data = new[,] { { 1, 2 }, { 3, 4 } };
        string[] headers = ["A", "B"];

        string result = Csv.SerializeWithHeader(headers, data);

        Assert.AreEqual("A,B\n1,2\n3,4", result);
    }

    [TestMethod]
    public void SerializeWithHeader_QuotesHeadersWithSeparators()
    {
        int[,] data = new[,] { { 1 } };
        string[] headers = ["a,b"];

        string result = Csv.SerializeWithHeader(headers, data);

        Assert.AreEqual("\"a,b\"\n1", result);
    }

    [TestMethod]
    public void SerializeWithHeader_HeaderCountMismatch_Throws()
    {
        int[,] data = new[,] { { 1, 2 } };
        string[] headers = ["only-one"];

        Assert.ThrowsExactly<ArgumentException>(() => Csv.SerializeWithHeader(headers, data));
    }

    [TestMethod]
    public void DeserializeWithHeader_SplitsHeaderFromData()
    {
        (string[] headers, int[,] data) = Csv.DeserializeWithHeader<int>("X,Y\n10,20\n30,40");

        CollectionAssert.AreEqual(new[] { "X", "Y" }, headers);
        Assert.AreEqual(2, data.GetLength(0));
        Assert.AreEqual(10, data[0, 0]);
        Assert.AreEqual(40, data[1, 1]);
    }

    [TestMethod]
    public void DeserializeWithHeader_EmptyInput_ReturnsEmpty()
    {
        (string[] headers, int[,] data) = Csv.DeserializeWithHeader<int>("");

        Assert.IsEmpty(headers);
        Assert.AreEqual(0, data.GetLength(0));
    }

    [TestMethod]
    public void RoundTrip_SerializeWithHeader_PreservesData()
    {
        string[] headers = ["Name", "Score", "Notes"];
        string[,] data = new[,]
        {
            { "Alice", "10", "first" },
            { "Bob", "20", "had, a comma" },
        };

        string csv = Csv.SerializeWithHeader(headers, data);
        (string[] outHeaders, string[,] outData) = Csv.DeserializeWithHeader<string>(csv);

        CollectionAssert.AreEqual(headers, outHeaders);
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Assert.AreEqual(data[row, col], outData[row, col]);
            }
        }
    }
}
