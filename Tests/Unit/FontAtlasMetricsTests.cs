using System.Collections.Generic;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FontAtlasMetricsTests
{
    private static FontAtlasMetrics MakeMetrics(int highestChar, Dictionary<char, Box2i> charPositions) =>
        new(0, highestChar, charPositions, []);

    private static Dictionary<char, Box2i> MakePositions(params (char c, int width)[] entries)
    {
        Dictionary<char, Box2i> dict = [];

        foreach (var (c, width) in entries)
        {
            dict[c] = new Box2i(0, 0, width, 10);
        }

        return dict;
    }

    // MeasureString(char)

    [TestMethod]
    public void MeasureString_Char_ReturnsCharWidthAndHighestChar()
    {
        FontAtlasMetrics metrics = MakeMetrics(20, MakePositions(('A', 12)));

        Vector2i result = metrics.MeasureString('A');

        Assert.AreEqual(12, result.X);
        Assert.AreEqual(20, result.Y);
    }

    // MeasureString(string)

    [TestMethod]
    public void MeasureString_EmptyString_ReturnszeroWidthAndHighestChar()
    {
        FontAtlasMetrics metrics = MakeMetrics(20, MakePositions());

        Vector2i result = metrics.MeasureString(string.Empty);

        Assert.AreEqual(0, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void MeasureString_SingleChar_ReturnsThatCharWidth()
    {
        FontAtlasMetrics metrics = MakeMetrics(15, MakePositions(('B', 8)));

        Vector2i result = metrics.MeasureString("B");

        Assert.AreEqual(8, result.X);
        Assert.AreEqual(15, result.Y);
    }

    [TestMethod]
    public void MeasureString_MultipleChars_ReturnsSumOfWidths()
    {
        FontAtlasMetrics metrics = MakeMetrics(15, MakePositions(('A', 10), ('B', 8), ('C', 12)));

        Vector2i result = metrics.MeasureString("ABC");

        Assert.AreEqual(30, result.X);
        Assert.AreEqual(15, result.Y);
    }

    [TestMethod]
    public void MeasureString_RepeatedChar_SumsWidthEachOccurrence()
    {
        FontAtlasMetrics metrics = MakeMetrics(15, MakePositions(('A', 10)));

        Vector2i result = metrics.MeasureString("AAA");

        Assert.AreEqual(30, result.X);
    }

    [TestMethod]
    public void MeasureString_YIsAlwaysHighestChar_RegardlessOfText()
    {
        FontAtlasMetrics metrics = MakeMetrics(25, MakePositions(('x', 5)));

        Assert.AreEqual(25, metrics.MeasureString("x").Y);
        Assert.AreEqual(25, metrics.MeasureString("xx").Y);
    }
}
