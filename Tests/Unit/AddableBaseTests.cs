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
            // Real Container.Remove calls SetParent(null) on the entity; mirror that so the
            // post-remove state is consistent with production behaviour.
            entity.SetParent(null);
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

    // SetParent attach / detach

    [TestMethod]
    public void SetParent_Attaches_WhenPreviouslyNull()
    {
        var a = new TestAddable();
        var c = new FakeContainer();

        a.SetParent(c);

        Assert.AreSame(c, a.Parent);
        Assert.IsTrue(a.Exists);
    }

    [TestMethod]
    public void SetParent_Null_Detaches_WhenPreviouslyAttached()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.SetParent(c);

        a.SetParent(null);

        Assert.IsNull(a.Parent);
        Assert.IsFalse(a.Exists);
    }

    [TestMethod]
    public void SetParent_NewContainer_WhileAlreadyAttached_Throws()
    {
        var a = new TestAddable();
        a.SetParent(new FakeContainer());

        Assert.ThrowsExactly<InvalidOperationException>(() => a.SetParent(new FakeContainer()));
    }

    [TestMethod]
    public void SetParent_Null_WhileAlreadyDetached_Throws()
    {
        var a = new TestAddable();

        Assert.ThrowsExactly<NullReferenceException>(() => a.SetParent(null));
    }

    // Remove

    [TestMethod]
    public void Remove_DelegatesToParentContainer_AndDetaches()
    {
        var a = new TestAddable();
        var c = new FakeContainer();
        a.SetParent(c);

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
