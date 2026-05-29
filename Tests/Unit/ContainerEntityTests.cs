using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ContainerEntityTests
{
    private sealed class TestContainerEntity : ContainerEntity { }

    private sealed class StubUpdateable : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public int UpdateCalls { get; private set; }
        public void Update(double elapsed) => UpdateCalls++;
    }

    private sealed class StubRenderable : AddableBase, IRenderable
    {
        public bool Visible { get; set; } = true;
        public int RenderCalls { get; private set; }
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) => RenderCalls++;
    }

    private sealed class FakeParent : IContainer
    {
        public (float x, float y) Translation { get; set; } = (0f, 0f);

        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x + Translation.x, y + Translation.y);
    }

    // Defaults

    [TestMethod]
    public void Defaults_ActiveAndVisibleAreTrue_LayerIsZero()
    {
        var ce = new TestContainerEntity();
        Assert.IsTrue(ce.Active);
        Assert.IsTrue(ce.Visible);
        Assert.AreEqual(0, ce.Layer);
    }

    // Layer

    [TestMethod]
    public void Layer_Set_StoresValue()
    {
        var ce = new TestContainerEntity();
        ce.Layer = 5;
        Assert.AreEqual(5, ce.Layer);
    }

    [TestMethod]
    public void Layer_Set_FiresLayerChangedWithOldAndNew()
    {
        var ce = new TestContainerEntity();
        LayerChangedEventArgs? received = null;
        ce.LayerChanged += (_, args) => received = args;

        ce.Layer = 7;

        Assert.IsNotNull(received);
        Assert.AreEqual(0, received.OldLayer);
        Assert.AreEqual(7, received.NewLayer);
    }

    [TestMethod]
    public void Layer_Set_ToSameValue_DoesNotFireEvent()
    {
        var ce = new TestContainerEntity();
        ce.Layer = 5;
        bool fired = false;
        ce.LayerChanged += (_, _) => fired = true;

        ce.Layer = 5;

        Assert.IsFalse(fired);
    }

    [TestMethod]
    public void Layer_Set_Negative_Throws()
    {
        var ce = new TestContainerEntity();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ce.Layer = -1);
    }

    // Update

    [TestMethod]
    public void Update_CallsUpdateOnEveryActiveChild()
    {
        var ce = new TestContainerEntity();
        ce.Parent = new FakeParent();
        var a = new StubUpdateable();
        var b = new StubUpdateable();
        ce.Add(a);
        ce.Add(b);

        ce.Update(0.016);

        Assert.AreEqual(1, a.UpdateCalls);
        Assert.AreEqual(1, b.UpdateCalls);
    }

    [TestMethod]
    public void Update_SkipsInactiveChildren()
    {
        var ce = new TestContainerEntity();
        ce.Parent = new FakeParent();
        var active = new StubUpdateable { Active = true };
        var inactive = new StubUpdateable { Active = false };
        ce.Add(active);
        ce.Add(inactive);

        ce.Update(0.016);

        Assert.AreEqual(1, active.UpdateCalls);
        Assert.AreEqual(0, inactive.UpdateCalls);
    }

    [TestMethod]
    public void Update_SkipsChildrenNotConnectedToTree()
    {
        // Bare ContainerEntity is unrooted — its children are not connected to any tree and must be skipped.
        var ce = new TestContainerEntity();
        var child = new StubUpdateable();
        ce.Add(child);

        ce.Update(0.016);

        Assert.AreEqual(0, child.UpdateCalls);
    }

    /// <summary>Child whose Update detaches the configured target from its parent.</summary>
    private sealed class DetachingUpdateable(IAddable target) : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public void Update(double elapsed)
        {
            target.Parent = null;
        }
    }

    [TestMethod]
    public void Update_WhenChildDetachesSelf_StopsIteratingRemainingChildren()
    {
        var parent = new FakeParent { Translation = (0f, 0f) };
        var ce = new TestContainerEntity();
        ce.Parent = parent;

        var laterChild = new StubUpdateable();
        ce.Add(new DetachingUpdateable(ce));
        ce.Add(laterChild);

        ce.Update(0.016);

        Assert.AreEqual(0, laterChild.UpdateCalls);
    }

    [TestMethod]
    public void Update_WhenGrandchildDetachesAncestor_StopsIteratingRemainingChildrenAtSiblingLevel()
    {
        // Topology: root → ce (grandparent) → middle → [detacher, laterSibling]
        // detacher removes ce from root. middle.IsConnectedToTree is then false (chain broken at ce),
        // so middle's loop must break before reaching laterSibling.
        var root = new FakeParent { Translation = (0f, 0f) };
        var ce = new TestContainerEntity();
        ce.Parent = root;

        var middle = new TestContainerEntity();
        ce.Add(middle);

        var laterSibling = new StubUpdateable();
        middle.Add(new DetachingUpdateable(ce));
        middle.Add(laterSibling);

        ce.Update(0.016);

        Assert.AreEqual(0, laterSibling.UpdateCalls);
    }

    // Render

    [TestMethod]
    public void Render_CallsRenderOnEveryVisibleChild()
    {
        var ce = new TestContainerEntity();
        var a = new StubRenderable();
        var b = new StubRenderable();
        ce.Add(a);
        ce.Add(b);

        var proj = Matrix3.Identity;
        var mv = Matrix3.Identity;
        ce.Render(ref proj, ref mv);

        Assert.AreEqual(1, a.RenderCalls);
        Assert.AreEqual(1, b.RenderCalls);
    }

    [TestMethod]
    public void Render_SkipsInvisibleChildren()
    {
        var ce = new TestContainerEntity();
        var visible = new StubRenderable { Visible = true };
        var hidden = new StubRenderable { Visible = false };
        ce.Add(visible);
        ce.Add(hidden);

        var proj = Matrix3.Identity;
        var mv = Matrix3.Identity;
        ce.Render(ref proj, ref mv);

        Assert.AreEqual(1, visible.RenderCalls);
        Assert.AreEqual(0, hidden.RenderCalls);
    }

    // GetWindowCoordinates

    [TestMethod]
    public void GetWindowCoordinates_WithoutParent_Throws()
    {
        var containerEntity = new TestContainerEntity();
        Assert.ThrowsExactly<InvalidOperationException>(() => containerEntity.GetWindowCoordinates(3f, 4f));
    }

    [TestMethod]
    public void GetWindowCoordinates_AfterRemoval_Throws()
    {
        var containerEntity = new TestContainerEntity();
        var parent = new FakeParent { Translation = (10f, 20f) };
        containerEntity.Parent = parent;
        containerEntity.Parent = null;

        Assert.ThrowsExactly<InvalidOperationException>(() => containerEntity.GetWindowCoordinates(3f, 4f));
    }

    [TestMethod]
    public void GetWindowCoordinates_WithParent_DelegatesUpward()
    {
        var containerEntity = new TestContainerEntity();
        var parent = new FakeParent { Translation = (10f, 20f) };
        containerEntity.Parent = parent;

        var (x, y) = containerEntity.GetWindowCoordinates(3f, 4f);

        Assert.AreEqual(13f, x);
        Assert.AreEqual(24f, y);
    }

    // Add / Remove / RemoveAll delegate to internal Container

    [TestMethod]
    public void Add_SetsParentToContainerEntity()
    {
        var ce = new TestContainerEntity();
        var child = new StubUpdateable();

        ce.Add(child);

        Assert.AreSame(ce, child.Parent);
    }

    [TestMethod]
    public void Remove_DetachesChild()
    {
        var ce = new TestContainerEntity();
        var child = new StubUpdateable();
        ce.Add(child);

        ce.Remove(child);

        Assert.IsNull(child.Parent);
    }

    [TestMethod]
    public void RemoveAll_DetachesEveryChild()
    {
        var ce = new TestContainerEntity();
        var a = new StubUpdateable();
        var b = new StubRenderable();
        ce.Add(a);
        ce.Add(b);

        ce.RemoveAll();

        Assert.IsNull(a.Parent);
        Assert.IsNull(b.Parent);
    }
}
