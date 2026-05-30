using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CircleShapeTests
{
    // Construction

    [TestMethod]
    public void Constructor_ExposesCentreAndRadius()
    {
        CircleShape shape = new(1, 2, 3);

        Assert.AreEqual(1f, shape.CentreX);
        Assert.AreEqual(2f, shape.CentreY);
        Assert.AreEqual(3f, shape.Radius);
    }

    [TestMethod]
    public void Constructor_FromPoint_CopiesCentre()
    {
        CircleShape shape = new(new Point(4, 5), 6);

        Assert.AreEqual(4f, shape.CentreX);
        Assert.AreEqual(5f, shape.CentreY);
        Assert.AreEqual(6f, shape.Radius);
    }

    [TestMethod]
    public void Centre_ReturnsCentrePoint()
    {
        CircleShape shape = new(1, 2, 3);
        Assert.AreEqual(new Point(1, 2), shape.Centre);
    }

    // Bounds

    [TestMethod]
    public void Bounds_EnclosesCircle()
    {
        CircleShape shape = new(10, 20, 5);
        Rect bounds = shape.Bounds;

        Assert.AreEqual(5f, bounds.X);
        Assert.AreEqual(15f, bounds.Y);
        Assert.AreEqual(10f, bounds.W);
        Assert.AreEqual(10f, bounds.H);
    }

    // ContainsPoint

    [TestMethod]
    public void ContainsPoint_WhenInside_ReturnsTrue()
    {
        CircleShape shape = new(0, 0, 10);
        Assert.IsTrue(shape.ContainsPoint(new(5, 0)));
    }

    [TestMethod]
    public void ContainsPoint_WhenOutside_ReturnsFalse()
    {
        CircleShape shape = new(0, 0, 10);
        Assert.IsFalse(shape.ContainsPoint(new(11, 0)));
    }

    // Overlaps — dispatch

    [TestMethod]
    public void Overlaps_CircleShape_DispatchesToCircleVsCircle()
    {
        CircleShape a = new(0, 0, 10);
        CircleShape b = new(5, 0, 10);

        Assert.IsTrue(a.Overlaps(b));
    }

    [TestMethod]
    public void Overlaps_RectShape_DispatchesToRectVsCircle()
    {
        CircleShape circle = new(5, 5, 3);
        RectShape rect = new(0, 0, 10, 10);

        Assert.IsTrue(circle.Overlaps(rect));
    }

    [TestMethod]
    public void Overlaps_UnsupportedShape_Throws()
    {
        CircleShape circle = new(0, 0, 10);
        StubShape stub = new();

        Assert.ThrowsExactly<NotSupportedException>(() => circle.Overlaps(stub));
    }

    // Translate

    [TestMethod]
    public void Translate_ShiftsCentre_PreservesRadius()
    {
        CircleShape original = new(1, 2, 3);
        var translated = (CircleShape)original.Translate(10, 20);

        Assert.AreEqual(11f, translated.CentreX);
        Assert.AreEqual(22f, translated.CentreY);
        Assert.AreEqual(3f, translated.Radius);
    }

    [TestMethod]
    public void Translate_DoesNotMutateOriginal()
    {
        CircleShape original = new(1, 2, 3);
        original.Translate(10, 20);

        Assert.AreEqual(1f, original.CentreX);
        Assert.AreEqual(2f, original.CentreY);
    }

    private sealed class StubShape : ICollisionShape
    {
        public Rect Bounds => Rect.EmptyRect;
        public bool ContainsPoint(Point point) => false;
        public bool Overlaps(ICollisionShape other) => false;
        public ICollisionShape Translate(float dx, float dy) => this;
    }
}
