using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Collision;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CollisionSolverTests
{
    private sealed class FakeOwner(float x, float y, float width, float height) : AddableRectBase(x, y, width, height)
    {
    }

    private World _world = null!;
    private CollisionSolver _solver = null!;
    private List<string> _events = null!;

    [TestInitialize]
    public void Setup()
    {
        _world = new World();
        _solver = new CollisionSolver();
        _world.Add(_solver);
        _events = [];
    }

    private Collider AddCollider(string label, float x, float y, float w, float h, uint category = 1, uint collidesWith = uint.MaxValue)
    {
        FakeOwner owner = new(x, y, w, h);
        Collider collider = new(_solver, owner, new RectShape(0, 0, w, h), category, collidesWith);
        collider.OverlapEntered += (_, e) => _events.Add($"{label}.Entered({Identify(e.Other)})");
        collider.OverlapExited += (_, e) => _events.Add($"{label}.Exited({Identify(e.Other)})");
        _world.Add(collider);
        _labels[collider] = label;
        return collider;
    }

    private readonly Dictionary<Collider, string> _labels = [];

    private string Identify(Collider c) => _labels.GetValueOrDefault(c, "?");

    // No colliders / no overlaps

    [TestMethod]
    public void Update_NoColliders_DoesNotThrow()
    {
        _solver.Update(0.016);
    }

    [TestMethod]
    public void Update_SingleCollider_FiresNoEvents()
    {
        AddCollider("A", 0, 0, 10, 10);

        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_TwoNonOverlapping_FiresNoEvents()
    {
        AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 100, 100, 10, 10);

        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    // Overlap entered

    [TestMethod]
    public void Update_TwoOverlapping_FiresEnteredOnBothSidesOnce()
    {
        AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);

        Assert.HasCount(2, _events);
        Assert.Contains("A.Entered(B)", _events);
        Assert.Contains("B.Entered(A)", _events);
    }

    [TestMethod]
    public void Update_StillOverlapping_DoesNotRefireEntered()
    {
        AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);
        _events.Clear();
        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    // Overlap exited

    [TestMethod]
    public void Update_AfterSeparation_FiresExitedOnBothSidesOnce()
    {
        Collider a = AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);
        _events.Clear();
        ((FakeOwner)a.Owner).X = 100;
        _solver.Update(0.016);

        Assert.HasCount(2, _events);
        Assert.Contains("A.Exited(B)", _events);
        Assert.Contains("B.Exited(A)", _events);
    }

    [TestMethod]
    public void Update_AlreadySeparated_DoesNotRefireExited()
    {
        Collider a = AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);
        ((FakeOwner)a.Owner).X = 100;
        _solver.Update(0.016);
        _events.Clear();
        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    // Enter / exit pair

    [TestMethod]
    public void Update_EnterThenExit_FiresBothInOrder()
    {
        Collider a = AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);
        ((FakeOwner)a.Owner).X = 100;
        _solver.Update(0.016);

        // Frame 1: 2 Entered. Frame 2: 2 Exited.
        Assert.HasCount(4, _events);
        Assert.IsTrue(_events.IndexOf("A.Entered(B)") < _events.IndexOf("A.Exited(B)"));
    }

    // Inactive collider

    [TestMethod]
    public void Update_InactiveCollider_DoesNotFireEvents()
    {
        Collider a = AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);
        a.Active = false;

        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_ColliderDeactivatedMidOverlap_FiresExitedOnSurvivor()
    {
        Collider a = AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);
        _events.Clear();
        a.Active = false;
        _solver.Update(0.016);

        Assert.Contains("A.Exited(B)", _events);
        Assert.Contains("B.Exited(A)", _events);
    }

    // Layer/Mask filtering

    [TestMethod]
    public void Update_CollideCategoriesExcludesOther_DoesNotFire()
    {
        // A is category 1, willing to collide only with category 2. B is category 4, collides with all.
        // (A.cat=1 & B.collides=all) ok but (B.cat=4 & A.collides=2) == 0.
        AddCollider("A", 0, 0, 10, 10, category: 1, collidesWith: 2);
        AddCollider("B", 5, 5, 10, 10, category: 4, collidesWith: uint.MaxValue);

        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    [TestMethod]
    public void Update_MutuallyOptedIn_Fires()
    {
        AddCollider("A", 0, 0, 10, 10, category: 1, collidesWith: 2);
        AddCollider("B", 5, 5, 10, 10, category: 2, collidesWith: 1);

        _solver.Update(0.016);

        Assert.HasCount(2, _events);
    }

    [TestMethod]
    public void Update_OneSidedCollideCategories_DoesNotFire()
    {
        // A wants B (A.collides covers B.category) but B does not want A.
        AddCollider("A", 0, 0, 10, 10, category: 1, collidesWith: 2);
        AddCollider("B", 5, 5, 10, 10, category: 2, collidesWith: 4);

        _solver.Update(0.016);

        Assert.IsEmpty(_events);
    }

    // Removal mid-overlap

    [TestMethod]
    public void Update_ColliderRemovedMidOverlap_SurvivorReceivesExited()
    {
        Collider a = AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);

        _solver.Update(0.016);
        _events.Clear();
        _world.Remove(a);
        _solver.Update(0.016);

        Assert.Contains("B.Exited(A)", _events);
    }

    // Multiple independent pairs

    [TestMethod]
    public void Update_TwoIndependentPairs_FireSeparately()
    {
        AddCollider("A", 0, 0, 10, 10);
        AddCollider("B", 5, 5, 10, 10);
        AddCollider("C", 100, 100, 10, 10);
        AddCollider("D", 105, 105, 10, 10);

        _solver.Update(0.016);

        Assert.HasCount(4, _events);
        Assert.Contains("A.Entered(B)", _events);
        Assert.Contains("B.Entered(A)", _events);
        Assert.Contains("C.Entered(D)", _events);
        Assert.Contains("D.Entered(C)", _events);
    }

    // Mixed shapes

    [TestMethod]
    public void Update_RectAndCircleOverlap_Fires()
    {
        FakeOwner rectOwner = new(0, 0, 20, 20);
        Collider rectCollider = new(_solver, rectOwner, new RectShape(0, 0, 20, 20));
        rectCollider.OverlapEntered += (_, _) => _events.Add("Rect.Entered");
        _world.Add(rectCollider);

        FakeOwner circleOwner = new(15, 10, 0, 0);
        Collider circleCollider = new(_solver, circleOwner, new CircleShape(0, 0, 10));
        circleCollider.OverlapEntered += (_, _) => _events.Add("Circle.Entered");
        _world.Add(circleCollider);

        _solver.Update(0.016);

        Assert.HasCount(2, _events);
    }
}
