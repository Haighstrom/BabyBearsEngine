using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Collision;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ColliderTests
{
    private sealed class FakeOwner(float x, float y, float width, float height) : AddableRectBase(x, y, width, height)
    {
    }

    private World _world = null!;
    private CollisionSolver _solver = null!;

    [TestInitialize]
    public void Setup()
    {
        _world = new World();
        _solver = new CollisionSolver();
        _world.Add(_solver);
    }

    // Defaults

    [TestMethod]
    public void Defaults_ActiveIsTrue()
    {
        FakeOwner owner = new(0, 0, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(0, 0, 10, 10));

        Assert.IsTrue(collider.Active);
    }

    [TestMethod]
    public void Defaults_CollisionCategoryIsOne_CollideCategoriesIsMax()
    {
        FakeOwner owner = new(0, 0, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(0, 0, 10, 10));

        Assert.AreEqual(1u, collider.CollisionCategory);
        Assert.AreEqual(uint.MaxValue, collider.CollideCategories);
    }

    [TestMethod]
    public void Constructor_ExposesOwnerAndLocalShape()
    {
        FakeOwner owner = new(0, 0, 10, 10);
        RectShape shape = new(0, 0, 10, 10);
        Collider collider = new(_solver, owner, shape);

        Assert.AreSame(owner, collider.Owner);
        Assert.AreSame(shape, collider.LocalShape);
    }

    // WorldShape

    [TestMethod]
    public void WorldShape_TranslatesRectShapeByOwnerPosition()
    {
        FakeOwner owner = new(100, 50, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(0, 0, 10, 10));

        var world = (RectShape)collider.WorldShape;

        Assert.AreEqual(100f, world.X);
        Assert.AreEqual(50f, world.Y);
        Assert.AreEqual(10f, world.Width);
        Assert.AreEqual(10f, world.Height);
    }

    [TestMethod]
    public void WorldShape_HonoursLocalOffset()
    {
        FakeOwner owner = new(100, 50, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(2, 3, 6, 6));

        var world = (RectShape)collider.WorldShape;

        Assert.AreEqual(102f, world.X);
        Assert.AreEqual(53f, world.Y);
    }

    [TestMethod]
    public void WorldShape_TranslatesCircleShapeByOwnerPosition()
    {
        FakeOwner owner = new(100, 50, 0, 0);
        Collider collider = new(_solver, owner, new CircleShape(5, 5, 8));

        var world = (CircleShape)collider.WorldShape;

        Assert.AreEqual(105f, world.CentreX);
        Assert.AreEqual(55f, world.CentreY);
        Assert.AreEqual(8f, world.Radius);
    }

    [TestMethod]
    public void WorldShape_FollowsOwnerWhenMoved()
    {
        FakeOwner owner = new(0, 0, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(0, 0, 10, 10));

        owner.X = 50;
        owner.Y = 25;
        var world = (RectShape)collider.WorldShape;

        Assert.AreEqual(50f, world.X);
        Assert.AreEqual(25f, world.Y);
    }

    // Registration lifecycle

    [TestMethod]
    public void OnAdded_RegistersWithCollisionSolver()
    {
        FakeOwner owner = new(0, 0, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(0, 0, 10, 10));

        Assert.DoesNotContain(collider, _solver.Colliders);

        _world.Add(collider);

        Assert.Contains(collider, _solver.Colliders);
    }

    [TestMethod]
    public void OnRemoved_DeregistersFromCollisionSolver()
    {
        FakeOwner owner = new(0, 0, 10, 10);
        Collider collider = new(_solver, owner, new RectShape(0, 0, 10, 10));
        _world.Add(collider);

        _world.Remove(collider);

        Assert.DoesNotContain(collider, _solver.Colliders);
    }
}
