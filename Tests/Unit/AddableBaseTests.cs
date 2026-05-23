using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AddableBaseTests
{
    private sealed class TestAddable : AddableBase
    {
        public int OnAddedCallCount { get; private set; } = 0;
        public int OnRemovedCallCount { get; private set; } = 0;

        protected override void OnAdded() => OnAddedCallCount++;

        protected override void OnRemoved() => OnRemovedCallCount++;
    }

    private sealed class FakeContainer : IContainer
    {
        public List<IAddable> RemovedFromHere { get; } = [];

        public void Add(IAddable entity) { }

        public void Remove(IAddable entity)
        {
            RemovedFromHere.Add(entity);
            // Real Container.Remove sets entity.Parent = null on the entity; mirror that so
            // the post-remove state is consistent with production behaviour.
            entity.Parent = null;
        }

        public void RemoveAll() { }

        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    // Initial state

    [TestMethod]
    public void NewAddable_HasNoParent_AndDoesNotExist()
    {
        var a = new TestAddable();
        Assert.IsNull(a.Parent);
        Assert.IsFalse(a.Exists);
    }

    // Parent attach / detach

    [TestMethod]
    public void Parent_Set_Attaches_WhenPreviouslyNull()
    {
        var a = new TestAddable();
        var c = new FakeContainer();

        a.Parent = c;

        Assert.AreSame(c, a.Parent);
        Assert.IsTrue(a.Exists);
    }

    [TestMethod]
    public void Parent_SetNull_Detaches_WhenPreviouslyAttached()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;

        a.Parent = null;

        Assert.IsNull(a.Parent);
        Assert.IsFalse(a.Exists);
    }

    [TestMethod]
    public void Parent_SetNewContainer_WhileAlreadyAttached_Throws()
    {
        var a = new TestAddable();
        a.Parent = new FakeContainer();

        Assert.ThrowsExactly<InvalidOperationException>(() => a.Parent = new FakeContainer());
    }

    [TestMethod]
    public void Parent_SetNull_WhileAlreadyDetached_Throws()
    {
        var a = new TestAddable();

        Assert.ThrowsExactly<NullReferenceException>(() => a.Parent = null);
    }

    // Remove

    [TestMethod]
    public void Remove_DelegatesToParentContainer_AndDetaches()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;

        a.Remove();

        Assert.AreSame(a, c.RemovedFromHere[0]);
        Assert.IsNull(a.Parent);
        Assert.IsFalse(a.Exists);
    }

    [TestMethod]
    public void Remove_WhenNoParent_Throws()
    {
        var a = new TestAddable();
        Assert.ThrowsExactly<NullReferenceException>(() => a.Remove());
    }

    // Added event

    [TestMethod]
    public void Added_Fires_WhenAttachedToParent()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        int callCount = 0;
        object? sender = null;
        a.Added += (s, _) => { callCount++; sender = s; };

        a.Parent = c;

        Assert.AreEqual(1, callCount);
        Assert.AreSame(a, sender);
    }

    [TestMethod]
    public void Added_DoesNotFire_WhenParentAssignmentThrows()
    {
        var a = new TestAddable();
        a.Parent = new FakeContainer();
        int callCount = 0;
        a.Added += (_, _) => callCount++;

        try
        {
            a.Parent = new FakeContainer();
        }
        catch (InvalidOperationException) { }

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void Added_FiresAfterOnAdded()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        int onAddedCountAtEventFire = -1;
        a.Added += (_, _) => onAddedCountAtEventFire = a.OnAddedCallCount;

        a.Parent = c;

        Assert.AreEqual(1, onAddedCountAtEventFire);
    }

    [TestMethod]
    public void Added_SupportsMultipleSubscribers()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        int firstCount = 0;
        int secondCount = 0;
        a.Added += (_, _) => firstCount++;
        a.Added += (_, _) => secondCount++;

        a.Parent = c;

        Assert.AreEqual(1, firstCount);
        Assert.AreEqual(1, secondCount);
    }

    // Removed event

    [TestMethod]
    public void Removed_Fires_WhenDetachedFromParent()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;
        int callCount = 0;
        object? sender = null;
        a.Removed += (s, _) => { callCount++; sender = s; };

        a.Parent = null;

        Assert.AreEqual(1, callCount);
        Assert.AreSame(a, sender);
    }

    [TestMethod]
    public void Removed_Fires_ViaRemoveHelper()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;
        int callCount = 0;
        a.Removed += (_, _) => callCount++;

        a.Remove();

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void Removed_DoesNotFire_WhenDetachThrows()
    {
        var a = new TestAddable();
        int callCount = 0;
        a.Removed += (_, _) => callCount++;

        try
        {
            a.Parent = null;
        }
        catch (NullReferenceException) { }

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void Removed_FiresAfterOnRemoved()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;
        int onRemovedCountAtEventFire = -1;
        a.Removed += (_, _) => onRemovedCountAtEventFire = a.OnRemovedCallCount;

        a.Parent = null;

        Assert.AreEqual(1, onRemovedCountAtEventFire);
    }

    [TestMethod]
    public void Removed_SupportsMultipleSubscribers()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;
        int firstCount = 0;
        int secondCount = 0;
        a.Removed += (_, _) => firstCount++;
        a.Removed += (_, _) => secondCount++;

        a.Parent = null;

        Assert.AreEqual(1, firstCount);
        Assert.AreEqual(1, secondCount);
    }

    // OnAdded / OnRemoved virtual hooks

    [TestMethod]
    public void OnAdded_IsCalled_WhenAttached()
    {
        var a = new TestAddable();
        var c = new FakeContainer();

        a.Parent = c;

        Assert.AreEqual(1, a.OnAddedCallCount);
    }

    [TestMethod]
    public void OnRemoved_IsCalled_WhenDetached()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.Parent = c;

        a.Parent = null;

        Assert.AreEqual(1, a.OnRemovedCallCount);
    }
}
