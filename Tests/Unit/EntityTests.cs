using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class EntityTests
{
    private sealed class CapturingRenderable : AddableBase, IRenderable
    {
        public bool Visible { get; set; } = true;
        public Matrix3 LastModelView { get; private set; }

        public void Render(ref Matrix3 projection, ref Matrix3 modelView)
        {
            LastModelView = modelView;
        }
    }

    private sealed class FakeParent : IContainer
    {
        public (float x, float y) ParentTranslation { get; set; } = (0f, 0f);

        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x + ParentTranslation.x, y + ParentTranslation.y);
    }

    // Constructor

    [TestMethod]
    public void Constructor_StoresPositionAndSize()
    {
        var e = new Entity(10, 20, 30, 40);
        Assert.AreEqual(10, e.X);
        Assert.AreEqual(20, e.Y);
        Assert.AreEqual(30, e.Width);
        Assert.AreEqual(40, e.Height);
    }

    // PositionOnScreen

    [TestMethod]
    public void PositionOnScreen_NoParent_UsesLocalXYAndSize()
    {
        var e = new Entity(10, 20, 30, 40);
        var rect = e.PositionOnScreen;
        Assert.AreEqual(10f, rect.X);
        Assert.AreEqual(20f, rect.Y);
        Assert.AreEqual(30f, rect.W);
        Assert.AreEqual(40f, rect.H);
    }

    [TestMethod]
    public void PositionOnScreen_WithParent_AddsParentTranslation()
    {
        var parent = new FakeParent { ParentTranslation = (100f, 200f) };
        var e = new Entity(10, 20, 30, 40);
        e.Parent = parent;

        var rect = e.PositionOnScreen;

        Assert.AreEqual(110f, rect.X);
        Assert.AreEqual(220f, rect.Y);
        Assert.AreEqual(30f, rect.W);
        Assert.AreEqual(40f, rect.H);
    }

    // GetWindowCoordinates

    [TestMethod]
    public void GetWindowCoordinates_NoParent_AddsEntityXY()
    {
        var e = new Entity(10, 20, 30, 40);
        var (x, y) = e.GetWindowCoordinates(3f, 4f);
        Assert.AreEqual(13f, x);
        Assert.AreEqual(24f, y);
    }

    [TestMethod]
    public void GetWindowCoordinates_WithParent_ChainsThroughParent()
    {
        var parent = new FakeParent { ParentTranslation = (100f, 200f) };
        var e = new Entity(10, 20, 30, 40);
        e.Parent = parent;

        var (x, y) = e.GetWindowCoordinates(3f, 4f);

        // (3 + 10) translated by parent = 113; (4 + 20) translated by parent = 224
        Assert.AreEqual(113f, x);
        Assert.AreEqual(224f, y);
    }

    // Render — translates modelView by entity (X, Y) before rendering children

    [TestMethod]
    public void Render_AppliesEntityXYTranslationToChildren()
    {
        var e = new Entity(10, 20, 0, 0);
        var child = new CapturingRenderable();
        e.Add(child);

        var proj = Matrix3.Identity;
        var mv = Matrix3.Identity;
        e.Render(ref proj, ref mv);

        // Child's recorded modelView, applied to (0, 0), should map to (10, 20).
        var childMv = child.LastModelView;
        var origin = Matrix3.Multiply(ref childMv, new Point(0f, 0f));
        Assert.AreEqual(10f, origin.X, 1e-4);
        Assert.AreEqual(20f, origin.Y, 1e-4);
    }
}
