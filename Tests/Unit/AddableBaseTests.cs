using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AddableBaseTests
{
    private sealed class TestAddable : AddableBase { }

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
}
