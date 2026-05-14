using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ContainerTests
{
    private sealed class StubAddable : AddableBase { }

    private sealed class StubUpdateable : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public int UpdateCalls { get; private set; }
        public void Update(double elapsed) => UpdateCalls++;
    }

    private sealed class StubRenderable : AddableBase, IRenderable
    {
        public bool Visible { get; set; } = true;
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class StubLayeredRenderable : AddableBase, IRenderable, ILayered
    {
        private int _layer;

        public StubLayeredRenderable(int initialLayer = 0) => _layer = initialLayer;

        public bool Visible { get; set; } = true;

        public int Layer
        {
            get => _layer;
            set
            {
                int old = _layer;
                _layer = value;
                if (old != value)
                {
                    LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<LayerChangedEventArgs>? LayerChanged;

        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class FakeRealParent : IContainer
    {
        public (float x, float y) GetWindowCoordinatesReturn { get; set; } = (0f, 0f);

        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => GetWindowCoordinatesReturn;
    }

    private static Container CreateContainer(out FakeRealParent realParent)
    {
        realParent = new FakeRealParent();
        return new Container(realParent);
    }

    // Add behaviour

    [TestMethod]
    public void Add_NullEntity_Throws()
    {
        var container = CreateContainer(out _);
        Assert.ThrowsExactly<ArgumentNullException>(() => container.Add(null!));
    }

    [TestMethod]
    public void Add_SetsParentToRealParent()
    {
        var container = CreateContainer(out var realParent);
        var entity = new StubAddable();

        container.Add(entity);

        Assert.AreSame(realParent, entity.Parent);
    }

    [TestMethod]
    public void Add_DuplicateEntity_Throws()
    {
        var container = CreateContainer(out _);
        var entity = new StubAddable();
        container.Add(entity);

        Assert.ThrowsExactly<InvalidOperationException>(() => container.Add(entity));
    }

    [TestMethod]
    public void Add_Updateable_AppearsInUpdatablesView()
    {
        var container = CreateContainer(out _);
        var u = new StubUpdateable();

        container.Add(u);

        Assert.AreSame(u, container.GetUpdatables().Single());
    }

    [TestMethod]
    public void Add_Renderable_AppearsInRenderablesView()
    {
        var container = CreateContainer(out _);
        var r = new StubRenderable();

        container.Add(r);

        Assert.AreSame(r, container.GetRenderables().Single());
    }

    // Remove behaviour

    [TestMethod]
    public void Remove_NullEntity_Throws()
    {
        var container = CreateContainer(out _);
        Assert.ThrowsExactly<ArgumentNullException>(() => container.Remove(null!));
    }

    [TestMethod]
    public void Remove_UnknownEntity_Throws()
    {
        var container = CreateContainer(out _);
        var entity = new StubAddable();

        Assert.ThrowsExactly<InvalidOperationException>(() => container.Remove(entity));
    }

    [TestMethod]
    public void Remove_ClearsParent_AndRemovesFromAllViews()
    {
        var container = CreateContainer(out _);
        var u = new StubUpdateable();
        var r = new StubRenderable();
        container.Add(u);
        container.Add(r);

        container.Remove(u);
        container.Remove(r);

        Assert.IsNull(u.Parent);
        Assert.IsNull(r.Parent);
        Assert.IsEmpty(container.GetUpdatables());
        Assert.IsEmpty(container.GetRenderables());
    }

    // RemoveAll

    [TestMethod]
    public void RemoveAll_DetachesEveryChild()
    {
        var container = CreateContainer(out _);
        var u = new StubUpdateable();
        var r = new StubRenderable();
        container.Add(u);
        container.Add(r);

        container.RemoveAll();

        Assert.IsNull(u.Parent);
        Assert.IsNull(r.Parent);
        Assert.IsEmpty(container.GetUpdatables());
        Assert.IsEmpty(container.GetRenderables());
    }

    [TestMethod]
    public void RemoveAll_OnEmptyContainer_IsNoOp()
    {
        var container = CreateContainer(out _);
        container.RemoveAll();
        Assert.IsEmpty(container.GetRenderables());
    }

    // Layer ordering

    [TestMethod]
    public void Add_LayeredRenderables_SortsHighLayerFirst()
    {
        // Higher layer = drawn first (further behind). So the renderables list orders
        // descending by layer.
        var container = CreateContainer(out _);
        var low = new StubLayeredRenderable(1);
        var high = new StubLayeredRenderable(5);
        var mid = new StubLayeredRenderable(3);

        container.Add(low);
        container.Add(high);
        container.Add(mid);

        var ordered = container.GetRenderables().ToList();
        Assert.AreSame(high, ordered[0]);
        Assert.AreSame(mid, ordered[1]);
        Assert.AreSame(low, ordered[2]);
    }

    [TestMethod]
    public void Add_NonLayeredRenderable_TreatedAsLayerZero()
    {
        var container = CreateContainer(out _);
        var nonLayered = new StubRenderable();
        var layered = new StubLayeredRenderable(5);

        container.Add(nonLayered);
        container.Add(layered);

        var ordered = container.GetRenderables().ToList();
        Assert.AreSame(layered, ordered[0]);     // layer 5 → behind
        Assert.AreSame(nonLayered, ordered[1]);  // layer 0 (default) → on top
    }

    [TestMethod]
    public void LayerChanged_AfterAdd_TriggersResort()
    {
        var container = CreateContainer(out _);
        var a = new StubLayeredRenderable(1);
        var b = new StubLayeredRenderable(2);
        container.Add(a);
        container.Add(b);
        Assert.AreSame(b, container.GetRenderables()[0]); // b on top initially

        a.Layer = 10; // a now has the highest layer

        var reordered = container.GetRenderables().ToList();
        Assert.AreSame(a, reordered[0]);
        Assert.AreSame(b, reordered[1]);
    }

    [TestMethod]
    public void Remove_LayeredRenderable_UnsubscribesFromLayerChanged()
    {
        var container = CreateContainer(out _);
        var a = new StubLayeredRenderable(1);
        var b = new StubLayeredRenderable(2);
        container.Add(a);
        container.Add(b);

        // Remove a, then change its layer. The container must NOT re-add it; it must
        // not be in the renderables list at all.
        container.Remove(a);
        a.Layer = 99;

        Assert.HasCount(1, container.GetRenderables());
        Assert.AreSame(b, container.GetRenderables().Single());
    }

    // GetWindowCoordinates delegation

    [TestMethod]
    public void GetWindowCoordinates_DelegatesToRealParent()
    {
        var container = CreateContainer(out var realParent);
        realParent.GetWindowCoordinatesReturn = (123f, 456f);

        var (x, y) = container.GetWindowCoordinates(1f, 2f);

        Assert.AreEqual(123f, x);
        Assert.AreEqual(456f, y);
    }
}
