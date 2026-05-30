using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RectShapeTests
{
    // Construction

    [TestMethod]
    public void Constructor_ExposesPositionAndSize()
    {
        RectShape shape = new(1, 2, 3, 4);

        Assert.AreEqual(1f, shape.X);
        Assert.AreEqual(2f, shape.Y);
        Assert.AreEqual(3f, shape.Width);
        Assert.AreEqual(4f, shape.Height);
    }

    [TestMethod]
    public void Constructor_FromRect_CopiesFields()
    {
        RectShape shape = new(new Rect(5, 6, 7, 8));

        Assert.AreEqual(5f, shape.X);
        Assert.AreEqual(6f, shape.Y);
        Assert.AreEqual(7f, shape.Width);
        Assert.AreEqual(8f, shape.Height);
    }

    [TestMethod]
    public void RightAndBottom_AreComputedFromPositionAndSize()
    {
        RectShape shape = new(1, 2, 3, 4);

        Assert.AreEqual(4f, shape.Right);
        Assert.AreEqual(6f, shape.Bottom);
    }

    // Bounds

    [TestMethod]
    public void Bounds_MatchesOwnRectangle()
    {
        RectShape shape = new(1, 2, 3, 4);
        Rect bounds = shape.Bounds;

        Assert.AreEqual(1f, bounds.X);
        Assert.AreEqual(2f, bounds.Y);
        Assert.AreEqual(3f, bounds.W);
        Assert.AreEqual(4f, bounds.H);
    }

    // ContainsPoint

    [TestMethod]
    public void ContainsPoint_WhenInside_ReturnsTrue()
    {
        RectShape shape = new(0, 0, 10, 10);
        Assert.IsTrue(shape.ContainsPoint(new(5, 5)));
    }

    [TestMethod]
    public void ContainsPoint_WhenOutside_ReturnsFalse()
    {
        RectShape shape = new(0, 0, 10, 10);
        Assert.IsFalse(shape.ContainsPoint(new(15, 5)));
    }

    // Overlaps — dispatch

    [TestMethod]
    public void Overlaps_RectShape_DispatchesToRectVsRect()
    {
        RectShape a = new(0, 0, 10, 10);
        RectShape b = new(5, 5, 10, 10);

        Assert.IsTrue(a.Overlaps(b));
    }

    [TestMethod]
    public void Overlaps_CircleShape_DispatchesToRectVsCircle()
    {
        RectShape rect = new(0, 0, 10, 10);
        CircleShape circle = new(15, 5, 10);

        Assert.IsTrue(rect.Overlaps(circle));
    }

    [TestMethod]
    public void Overlaps_UnsupportedShape_Throws()
    {
        RectShape rect = new(0, 0, 10, 10);
        StubShape stub = new();

        Assert.ThrowsExactly<NotSupportedException>(() => rect.Overlaps(stub));
    }

    // Translate

    [TestMethod]
    public void Translate_ShiftsPosition_PreservesSize()
    {
        RectShape original = new(1, 2, 3, 4);
        RectShape translated = (RectShape)original.Translate(10, 20);

        Assert.AreEqual(11f, translated.X);
        Assert.AreEqual(22f, translated.Y);
        Assert.AreEqual(3f, translated.Width);
        Assert.AreEqual(4f, translated.Height);
    }

    [TestMethod]
    public void Translate_DoesNotMutateOriginal()
    {
        RectShape original = new(1, 2, 3, 4);
        original.Translate(10, 20);

        Assert.AreEqual(1f, original.X);
        Assert.AreEqual(2f, original.Y);
    }

    private sealed class StubShape : ICollisionShape
    {
        public Rect Bounds => Rect.EmptyRect;
        public bool ContainsPoint(Point point) => false;
        public bool Overlaps(ICollisionShape other) => false;
        public ICollisionShape Translate(float dx, float dy) => this;
    }
}
