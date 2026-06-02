using System;
using System.Linq;
using BabyBearsEngine.Geometry;
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

    private sealed class StubUpdateableLast : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public bool UpdateLast => true;
        public int UpdateCalls { get; private set; }
        public void Update(double elapsed) => UpdateCalls++;
    }

    private sealed class StubMutableUpdateLast(bool initialUpdateLast) : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public bool UpdateLast { get; set; } = initialUpdateLast;
        public void Update(double elapsed) { }
    }

    private sealed class StubLayeredUpdateable(int initialLayer, bool updateLast = false) : AddableBase, IUpdateable, ILayered
    {
        public bool Active { get; set; } = true;
        public bool UpdateLast => updateLast;

        public int Layer
        {
            get => initialLayer;
            set
            {
                int old = initialLayer;
                initialLayer = value;
                if (old != value)
                {
                    LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<LayerChangedEventArgs>? LayerChanged;

        public void Update(double elapsed) { }
    }

    private sealed class StubRenderable : AddableBase, IRenderable
    {
        public bool Visible { get; set; } = true;
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class StubLayeredRenderable(int initialLayer = 0) : AddableBase, IRenderable, ILayered
    {
        public bool Visible { get; set; } = true;

        public int Layer
        {
            get => initialLayer;
            set
            {
                int old = initialLayer;
                initialLayer = value;
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
        Assert.ThrowsExactly<ArgumentNullException>(() => container.Add((IAddable)null!));
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
    public void Add_ParamsOverload_AddsAllChildren()
    {
        var container = CreateContainer(out var realParent);
        var a = new StubAddable();
        var b = new StubAddable();
        var c = new StubAddable();

        container.Add(a, b, c);

        Assert.AreSame(realParent, a.Parent);
        Assert.AreSame(realParent, b.Parent);
        Assert.AreSame(realParent, c.Parent);
    }

    [TestMethod]
    public void Add_ParamsOverload_PreservesOrder()
    {
        var container = CreateContainer(out _);
        var a = new StubUpdateable();
        var b = new StubUpdateable();
        var c = new StubUpdateable();

        container.Add(a, b, c);

        var updatables = container.GetUpdatables();
        Assert.AreEqual(3, updatables.Count);
        Assert.AreSame(a, updatables[0]);
        Assert.AreSame(b, updatables[1]);
        Assert.AreSame(c, updatables[2]);
    }

    [TestMethod]
    public void Add_ParamsOverload_DuplicateChild_Throws()
    {
        var container = CreateContainer(out _);
        var a = new StubAddable();
        var b = new StubAddable();
        container.Add(a);

        Assert.ThrowsExactly<InvalidOperationException>(() => container.Add(b, a));
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
    public void Add_NonLayeredRenderable_SortsBehindLayeredContent()
    {
        // Renderables that do not implement ILayered are treated as int.MaxValue,
        // placing them at the very back so they draw behind all layered content.
        var container = CreateContainer(out _);
        var nonLayered = new StubRenderable();
        var layered = new StubLayeredRenderable(5);

        container.Add(nonLayered);
        container.Add(layered);

        var ordered = container.GetRenderables().ToList();
        Assert.AreSame(nonLayered, ordered[0]);  // non-layered → int.MaxValue → drawn first (behind)
        Assert.AreSame(layered, ordered[1]);     // layer 5 → on top
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

    // UpdateLast routing

    [TestMethod]
    public void Add_UpdateLastTrue_RoutesIntoGetUpdatablesLast()
    {
        var container = CreateContainer(out _);
        var u = new StubUpdateableLast();

        container.Add(u);

        Assert.AreSame(u, container.GetUpdatablesLast().Single());
        Assert.IsEmpty(container.GetUpdatables());
    }

    [TestMethod]
    public void Add_UpdateLastDefaultFalse_RoutesIntoGetUpdatables()
    {
        // StubUpdateable does not override UpdateLast → uses the IUpdateable default of false.
        var container = CreateContainer(out _);
        var u = new StubUpdateable();

        container.Add(u);

        Assert.AreSame(u, container.GetUpdatables().Single());
        Assert.IsEmpty(container.GetUpdatablesLast());
    }

    [TestMethod]
    public void Add_MixedUpdateables_PopulatesBothBuckets()
    {
        var container = CreateContainer(out _);
        var regular = new StubUpdateable();
        var last = new StubUpdateableLast();

        container.Add(regular);
        container.Add(last);

        Assert.AreSame(regular, container.GetUpdatables().Single());
        Assert.AreSame(last, container.GetUpdatablesLast().Single());
    }

    [TestMethod]
    public void Remove_UpdateLastEntity_RemovesFromGetUpdatablesLast()
    {
        var container = CreateContainer(out _);
        var u = new StubUpdateableLast();
        container.Add(u);

        container.Remove(u);

        Assert.IsEmpty(container.GetUpdatablesLast());
        Assert.IsNull(u.Parent);
    }

    [TestMethod]
    public void RemoveAll_ClearsBothBuckets()
    {
        var container = CreateContainer(out _);
        var regular = new StubUpdateable();
        var last = new StubUpdateableLast();
        container.Add(regular);
        container.Add(last);

        container.RemoveAll();

        Assert.IsEmpty(container.GetUpdatables());
        Assert.IsEmpty(container.GetUpdatablesLast());
        Assert.IsNull(regular.Parent);
        Assert.IsNull(last.Parent);
    }

    [TestMethod]
    public void UpdateLast_MutationAfterAdd_DoesNotMoveBucket()
    {
        // Contract: UpdateLast is snapshotted at Add. Changing the property afterwards must
        // NOT move the updateable between buckets. The only way to switch buckets is to remove
        // and re-add.
        var container = CreateContainer(out _);
        var u = new StubMutableUpdateLast(initialUpdateLast: false);
        container.Add(u);

        u.UpdateLast = true;

        Assert.Contains(u, container.GetUpdatables());
        Assert.DoesNotContain(u, container.GetUpdatablesLast());
    }

    [TestMethod]
    public void LayerChanged_OnUpdateLastEntity_ReSortsWithinLastBucket()
    {
        // An UpdateLast entity whose layer changes should be re-sorted inside the last bucket
        // but must not migrate into the regular bucket.
        var container = CreateContainer(out _);
        var lowLast = new StubLayeredUpdateable(initialLayer: 1, updateLast: true);
        var highLast = new StubLayeredUpdateable(initialLayer: 5, updateLast: true);
        container.Add(lowLast);
        container.Add(highLast);

        // Sanity: high layer is iterated first (matches the renderable convention).
        var before = container.GetUpdatablesLast().ToList();
        Assert.AreSame(highLast, before[0]);
        Assert.AreSame(lowLast, before[1]);

        lowLast.Layer = 99;
        var after = container.GetUpdatablesLast().ToList();

        Assert.AreSame(lowLast, after[0]);
        Assert.AreSame(highLast, after[1]);
        Assert.IsEmpty(container.GetUpdatables());
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

    // Equal-layer tie-break: later-added overlays earlier-added (matches IRenderable doc).

    [TestMethod]
    public void GetRenderables_EqualLayer_LaterAddedRendersLater_SoItOverlaysEarlier()
    {
        var container = CreateContainer(out _);
        var first = new StubLayeredRenderable(5);
        var second = new StubLayeredRenderable(5);

        container.Add(first);
        container.Add(second);

        // GetRenderables iterates back-to-front: index 0 renders first (drawn behind),
        // index 1 renders second (drawn on top). Later-added must be at the higher index.
        var ordered = container.GetRenderables();
        Assert.HasCount(2, ordered);
        Assert.AreSame(first, ordered[0]);
        Assert.AreSame(second, ordered[1]);
    }

    [TestMethod]
    public void GetUpdatables_EqualLayer_LaterAddedUpdatesLast_SoItWinsMouseInputArbitration()
    {
        // MouseSolver registers the LAST-updated entity as topmost in the click stack — so the
        // same "later-added overlays" rule applies to updateable ordering.
        var container = CreateContainer(out _);
        var first = new StubLayeredUpdateable(5);
        var second = new StubLayeredUpdateable(5);

        container.Add(first);
        container.Add(second);

        var ordered = container.GetUpdatables();
        Assert.HasCount(2, ordered);
        Assert.AreSame(first, ordered[0]);
        Assert.AreSame(second, ordered[1]);
    }
}
