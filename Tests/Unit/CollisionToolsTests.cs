using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CollisionToolsTests
{
    // RectVsRect

    [TestMethod]
    public void RectVsRect_WhenOverlapping_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsRect(new(0, 0, 10, 10), new(5, 5, 10, 10)));
    }

    [TestMethod]
    public void RectVsRect_WhenContained_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsRect(new(0, 0, 100, 100), new(40, 40, 10, 10)));
    }

    [TestMethod]
    public void RectVsRect_WhenSeparated_ReturnsFalse()
    {
        Assert.IsFalse(CollisionTools.RectVsRect(new(0, 0, 10, 10), new(20, 0, 10, 10)));
    }

    [TestMethod]
    public void RectVsRect_WhenEdgesTouchHorizontally_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsRect(new(0, 0, 10, 10), new(10, 0, 10, 10)));
    }

    [TestMethod]
    public void RectVsRect_WhenEdgesTouchVertically_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsRect(new(0, 0, 10, 10), new(0, 10, 10, 10)));
    }

    [TestMethod]
    public void RectVsRect_WhenCornersTouchDiagonally_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsRect(new(0, 0, 10, 10), new(10, 10, 10, 10)));
    }

    // CircleVsCircle

    [TestMethod]
    public void CircleVsCircle_WhenOverlapping_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.CircleVsCircle(new(0, 0, 10), new(5, 0, 10)));
    }

    [TestMethod]
    public void CircleVsCircle_WhenContained_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.CircleVsCircle(new(0, 0, 100), new(10, 10, 5)));
    }

    [TestMethod]
    public void CircleVsCircle_WhenSeparated_ReturnsFalse()
    {
        Assert.IsFalse(CollisionTools.CircleVsCircle(new(0, 0, 10), new(30, 0, 10)));
    }

    [TestMethod]
    public void CircleVsCircle_WhenJustTouching_ReturnsTrue()
    {
        // Centres 20 apart, radii sum to 20 → exactly touching.
        Assert.IsTrue(CollisionTools.CircleVsCircle(new(0, 0, 10), new(20, 0, 10)));
    }

    [TestMethod]
    public void CircleVsCircle_WhenSeparatedByFraction_ReturnsFalse()
    {
        Assert.IsFalse(CollisionTools.CircleVsCircle(new(0, 0, 10), new(20.1f, 0, 10)));
    }

    // RectVsCircle

    [TestMethod]
    public void RectVsCircle_WhenCircleInsideRect_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsCircle(new(0, 0, 100, 100), new(50, 50, 5)));
    }

    [TestMethod]
    public void RectVsCircle_WhenRectInsideCircle_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsCircle(new(45, 45, 10, 10), new(50, 50, 100)));
    }

    [TestMethod]
    public void RectVsCircle_WhenCircleOverlapsEdge_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.RectVsCircle(new(0, 0, 10, 10), new(15, 5, 10)));
    }

    [TestMethod]
    public void RectVsCircle_WhenCircleOverlapsCorner_ReturnsTrue()
    {
        // Circle centred at (12, 12) radius 5 — distance from corner (10, 10) is sqrt(8) ≈ 2.83 < 5.
        Assert.IsTrue(CollisionTools.RectVsCircle(new(0, 0, 10, 10), new(12, 12, 5)));
    }

    [TestMethod]
    public void RectVsCircle_WhenSeparated_ReturnsFalse()
    {
        Assert.IsFalse(CollisionTools.RectVsCircle(new(0, 0, 10, 10), new(30, 30, 5)));
    }

    [TestMethod]
    public void RectVsCircle_WhenCircleJustMissesCorner_ReturnsFalse()
    {
        // Distance from (15, 15) to (10, 10) is sqrt(50) ≈ 7.07; radius 7 → miss.
        Assert.IsFalse(CollisionTools.RectVsCircle(new(0, 0, 10, 10), new(15, 15, 7)));
    }

    // PointVsRect

    [TestMethod]
    public void PointVsRect_WhenInside_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.PointVsRect(new(5, 5), new(0, 0, 10, 10)));
    }

    [TestMethod]
    public void PointVsRect_WhenOnEdge_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.PointVsRect(new(0, 5), new(0, 0, 10, 10)));
    }

    [TestMethod]
    public void PointVsRect_WhenOnCorner_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.PointVsRect(new(10, 10), new(0, 0, 10, 10)));
    }

    [TestMethod]
    public void PointVsRect_WhenOutside_ReturnsFalse()
    {
        Assert.IsFalse(CollisionTools.PointVsRect(new(11, 5), new(0, 0, 10, 10)));
    }

    // PointVsCircle

    [TestMethod]
    public void PointVsCircle_WhenInside_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.PointVsCircle(new(0, 0), new(0, 0, 10)));
    }

    [TestMethod]
    public void PointVsCircle_WhenOnBoundary_ReturnsTrue()
    {
        Assert.IsTrue(CollisionTools.PointVsCircle(new(10, 0), new(0, 0, 10)));
    }

    [TestMethod]
    public void PointVsCircle_WhenOutside_ReturnsFalse()
    {
        Assert.IsFalse(CollisionTools.PointVsCircle(new(11, 0), new(0, 0, 10)));
    }
}
