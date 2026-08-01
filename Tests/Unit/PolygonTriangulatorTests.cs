using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PolygonTriangulatorTests
{
    [TestMethod]
    public void Triangulate_Triangle_ReturnsExactlyOneTriangle()
    {
        Point[] triangle = [new(0, 0), new(10, 0), new(0, 10)];

        Point[] result = PolygonTriangulator.Triangulate(triangle);

        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void Triangulate_Hexagon_OutputLengthIsMultipleOfThree()
    {
        Point[] hexagon = RegularPolygon(6, 50f);

        Point[] result = PolygonTriangulator.Triangulate(hexagon);

        Assert.AreEqual(0, result.Length % 3);
    }

    [TestMethod]
    public void Triangulate_Square_TrianglesCoverExactArea()
    {
        Point[] square = [new(0, 0), new(10, 0), new(10, 10), new(0, 10)];

        Point[] result = PolygonTriangulator.Triangulate(square);

        Assert.AreEqual(100f, TotalSignedArea(result), 1e-3f);
    }

    [TestMethod]
    public void Triangulate_ClockwiseWoundSquare_StillCoversExactArea()
    {
        Point[] square = [new(0, 0), new(0, 10), new(10, 10), new(10, 0)]; // reversed winding

        Point[] result = PolygonTriangulator.Triangulate(square);

        Assert.AreEqual(100f, MathF.Abs(TotalSignedArea(result)), 1e-3f);
    }

    [TestMethod]
    public void Triangulate_ConcaveLShape_CoversExactArea()
    {
        // A 6x6 square with a 3x3 bite taken out of the top-right corner: area 36 - 9 = 27.
        Point[] lShape = [new(0, 0), new(6, 0), new(6, 3), new(3, 3), new(3, 6), new(0, 6)];

        Point[] result = PolygonTriangulator.Triangulate(lShape);

        Assert.AreEqual(27f, MathF.Abs(TotalSignedArea(result)), 1e-3f);
    }

    [TestMethod]
    public void Triangulate_FewerThanThreePoints_Throws()
    {
        Point[] twoPoints = [new(0, 0), new(10, 0)];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PolygonTriangulator.Triangulate(twoPoints));
    }

    private static Point[] RegularPolygon(int sideCount, float radius)
    {
        Point[] points = new Point[sideCount];
        for (int sideIndex = 0; sideIndex < sideCount; sideIndex++)
        {
            float angleRadians = sideIndex * (2f * MathF.PI / sideCount);
            points[sideIndex] = new Point(radius * MathF.Cos(angleRadians), radius * MathF.Sin(angleRadians));
        }

        return points;
    }

    private static float TotalSignedArea(Point[] triangleVertices)
    {
        float area = 0f;
        for (int triangleIndex = 0; triangleIndex < triangleVertices.Length; triangleIndex += 3)
        {
            Point a = triangleVertices[triangleIndex];
            Point b = triangleVertices[triangleIndex + 1];
            Point c = triangleVertices[triangleIndex + 2];
            area += 0.5f * ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X));
        }

        return area;
    }
}
