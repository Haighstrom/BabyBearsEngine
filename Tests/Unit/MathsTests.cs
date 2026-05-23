using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MathsTests
{
    // Clamp(float)

    [TestMethod]
    public void ClampFloat_BetweenBounds_ReturnsValue()
    {
        Assert.AreEqual(5f, Maths.Clamp(5f, 0f, 10f));
    }

    [TestMethod]
    public void ClampFloat_BelowMin_ReturnsMin()
    {
        Assert.AreEqual(0f, Maths.Clamp(-5f, 0f, 10f));
    }

    [TestMethod]
    public void ClampFloat_AboveMax_ReturnsMax()
    {
        Assert.AreEqual(10f, Maths.Clamp(15f, 0f, 10f));
    }

    // Clamp(int)

    [TestMethod]
    public void ClampInt_BetweenBounds_ReturnsValue()
    {
        Assert.AreEqual(5, Maths.Clamp(5, 0, 10));
    }

    [TestMethod]
    public void ClampInt_BelowMin_ReturnsMin()
    {
        Assert.AreEqual(0, Maths.Clamp(-1, 0, 10));
    }

    [TestMethod]
    public void ClampInt_AboveMax_ReturnsMax()
    {
        Assert.AreEqual(10, Maths.Clamp(11, 0, 10));
    }

    // DistGrid

    [TestMethod]
    public void DistGrid_SamePoint_Zero()
    {
        Point p = new(3, 4);

        Assert.AreEqual(0f, Maths.DistGrid(p, p));
    }

    [TestMethod]
    public void DistGrid_OffsetX_ReturnsX()
    {
        Assert.AreEqual(5f, Maths.DistGrid(new Point(0, 0), new Point(5, 0)));
    }

    [TestMethod]
    public void DistGrid_OffsetY_ReturnsY()
    {
        Assert.AreEqual(7f, Maths.DistGrid(new Point(0, 0), new Point(0, 7)));
    }

    [TestMethod]
    public void DistGrid_Manhattan_Sum()
    {
        Assert.AreEqual(7f, Maths.DistGrid(new Point(1, 1), new Point(4, 5)));
    }

    [TestMethod]
    public void DistGrid_NegativeOffsets_TakesAbsoluteValue()
    {
        Assert.AreEqual(7f, Maths.DistGrid(new Point(4, 5), new Point(1, 1)));
    }

    [TestMethod]
    public void DistGrid_IPositionOverload_ReturnsManhattanDistance()
    {
        IPosition a = new Point(1, 1);
        IPosition b = new Point(4, 5);

        Assert.AreEqual(7f, Maths.DistGrid(a, b));
    }

    // GetNearestSide — point outside the rect

    [TestMethod]
    public void GetNearestSide_PointAboveTop_ReturnsUp()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Up, Maths.GetNearestSide(rect, new Point(5, -5)));
    }

    [TestMethod]
    public void GetNearestSide_PointBelowBottom_ReturnsDown()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Down, Maths.GetNearestSide(rect, new Point(5, 15)));
    }

    [TestMethod]
    public void GetNearestSide_PointLeftOfLeft_ReturnsLeft()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Left, Maths.GetNearestSide(rect, new Point(-5, 5)));
    }

    [TestMethod]
    public void GetNearestSide_PointRightOfRight_ReturnsRight()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Right, Maths.GetNearestSide(rect, new Point(15, 5)));
    }

    // GetNearestSide — point inside the rect

    [TestMethod]
    public void GetNearestSide_PointInsideRectCloserToTop_ReturnsUp()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Up, Maths.GetNearestSide(rect, new Point(5, 1)));
    }

    [TestMethod]
    public void GetNearestSide_PointInsideRectCloserToBottom_ReturnsDown()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Down, Maths.GetNearestSide(rect, new Point(5, 9)));
    }

    [TestMethod]
    public void GetNearestSide_PointInsideRectCloserToLeft_ReturnsLeft()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Left, Maths.GetNearestSide(rect, new Point(1, 5)));
    }

    [TestMethod]
    public void GetNearestSide_PointInsideRectCloserToRight_ReturnsRight()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Right, Maths.GetNearestSide(rect, new Point(9, 5)));
    }

    // GetNearestSide — ties resolve to the documented order (Left, Right, Up, Down)

    [TestMethod]
    public void GetNearestSide_PointAtTopLeftCorner_ReturnsLeftByTieBreak()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Left, Maths.GetNearestSide(rect, new Point(0, 0)));
    }

    [TestMethod]
    public void GetNearestSide_PointAtCentre_ReturnsLeftByTieBreak()
    {
        Rect rect = new(0, 0, 10, 10);

        Assert.AreEqual(Direction.Left, Maths.GetNearestSide(rect, new Point(5, 5)));
    }
}
