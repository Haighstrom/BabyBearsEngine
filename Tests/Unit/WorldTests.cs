using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WorldTests
{
    private sealed class StubUpdateable : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public int UpdateCalls { get; private set; }
        public double LastElapsed { get; private set; }

        public void Update(double elapsed)
        {
            UpdateCalls++;
            LastElapsed = elapsed;
        }
    }

    private sealed class StubRenderable : AddableBase, IRenderable
    {
        public bool Visible { get; set; } = true;
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class OrderTrackingUpdateable(List<string> order, string label, bool updateLast = false) : AddableBase, IUpdateable
    {
        public bool Active { get; set; } = true;
        public bool UpdateLast => updateLast;
        public void Update(double elapsed) => order.Add(label);
    }

    // Note: World.Draw is intentionally not tested here — it issues GL calls and reads
    // Window.Width/Height through the static facade, both of which require booting the
    // engine. The non-rendering paths (Add/Remove/Update/GetWindowCoordinates) are
    // covered below.

    // Defaults

    [TestMethod]
    public void Defaults_BackgroundIsCornflowerBlue()
    {
        var world = new World();
        Assert.AreEqual(Colour.CornflowerBlue, world.BackgroundColour);
    }

    [TestMethod]
    public void BackgroundColour_IsSettable()
    {
        var world = new World();
        world.BackgroundColour = Colour.Red;
        Assert.AreEqual(Colour.Red, world.BackgroundColour);
    }

    // Lifecycle hooks (default impls are no-ops; just verify they don't throw)

    [TestMethod]
    public void Load_OnDefaultWorld_DoesNotThrow()
    {
        var world = new World();
        world.Load();
    }

    [TestMethod]
    public void Unload_OnDefaultWorld_DoesNotThrow()
    {
        var world = new World();
        world.Unload();
    }

    // Coordinate translation — World is the root, so local == window coordinates.

    [TestMethod]
    public void GetWindowCoordinates_ReturnsInputUnchanged()
    {
        var world = new World();
        var (x, y) = world.GetWindowCoordinates(123f, 456f);
        Assert.AreEqual(123f, x);
        Assert.AreEqual(456f, y);
    }

    // Container delegation

    [TestMethod]
    public void Add_SetsParentToWorld()
    {
        var world = new World();
        var entity = new StubUpdateable();

        world.Add(entity);

        Assert.AreSame(world, entity.Parent);
    }

    [TestMethod]
    public void Remove_DetachesEntity()
    {
        var world = new World();
        var entity = new StubUpdateable();
        world.Add(entity);

        world.Remove(entity);

        Assert.IsNull(entity.Parent);
    }

    [TestMethod]
    public void RemoveAll_DetachesEveryChild()
    {
        var world = new World();
        var u = new StubUpdateable();
        var r = new StubRenderable();
        world.Add(u);
        world.Add(r);

        world.RemoveAll();

        Assert.IsNull(u.Parent);
        Assert.IsNull(r.Parent);
    }

    // Update

    [TestMethod]
    public void Update_CallsUpdateOnEveryUpdateable_WithElapsedPassedThrough()
    {
        var world = new World();
        var a = new StubUpdateable();
        var b = new StubUpdateable();
        world.Add(a);
        world.Add(b);

        world.Update(0.025);

        Assert.AreEqual(1, a.UpdateCalls);
        Assert.AreEqual(0.025, a.LastElapsed);
        Assert.AreEqual(1, b.UpdateCalls);
        Assert.AreEqual(0.025, b.LastElapsed);
    }

    [TestMethod]
    public void Update_OnEmptyWorld_DoesNotThrow()
    {
        var world = new World();
        world.Update(0.016);
    }

    [TestMethod]
    public void Update_SkipsInactiveChildren()
    {
        var world = new World();
        var inactive = new StubUpdateable { Active = false };
        world.Add(inactive);

        world.Update(0.016);

        Assert.AreEqual(0, inactive.UpdateCalls);
    }

    // Overlay — a second container rendered/updated as a separate pass after the main scene.

    [TestMethod]
    public void Overlay_IsNonNull()
    {
        var world = new World();
        Assert.IsNotNull(world.Overlay);
    }

    [TestMethod]
    public void Overlay_IsDistinctFromMainContainer()
    {
        var world = new World();
        var mainChild = new StubUpdateable();
        var overlayChild = new StubUpdateable();

        world.Add(mainChild);
        world.Overlay.Add(overlayChild);

        // RemoveAll on the world's main container should not affect overlay children.
        world.RemoveAll();

        Assert.IsNull(mainChild.Parent);
        Assert.AreSame(world, overlayChild.Parent);
    }

    [TestMethod]
    public void Overlay_Add_SetsParentToWorld()
    {
        var world = new World();
        var entity = new StubUpdateable();

        world.Overlay.Add(entity);

        // Overlay shares the world's coordinate space, so parent points back to the world
        // (not at the overlay container itself).
        Assert.AreSame(world, entity.Parent);
    }

    [TestMethod]
    public void Update_AlsoUpdatesOverlayChildren()
    {
        var world = new World();
        var mainChild = new StubUpdateable();
        var overlayChild = new StubUpdateable();
        world.Add(mainChild);
        world.Overlay.Add(overlayChild);

        world.Update(0.025);

        Assert.AreEqual(1, mainChild.UpdateCalls);
        Assert.AreEqual(0.025, mainChild.LastElapsed);
        Assert.AreEqual(1, overlayChild.UpdateCalls);
        Assert.AreEqual(0.025, overlayChild.LastElapsed);
    }

    // UpdateLast ordering — regulars tick first, update-last entries tick after, regardless of add order.

    [TestMethod]
    public void Update_TicksRegularBeforeUpdateLast_RegardlessOfAddOrder()
    {
        var world = new World();
        var order = new List<string>();
        var last = new OrderTrackingUpdateable(order, "last", updateLast: true);
        var regular = new OrderTrackingUpdateable(order, "regular");

        // Add UpdateLast FIRST to prove ordering is bucket-driven, not insertion-driven.
        world.Add(last);
        world.Add(regular);

        world.Update(0.016);

        Assert.HasCount(2, order);
        Assert.AreEqual("regular", order[0]);
        Assert.AreEqual("last", order[1]);
    }

    [TestMethod]
    public void Update_TicksMainContainerFullyBeforeOverlay()
    {
        var world = new World();
        var order = new List<string>();
        world.Add(new OrderTrackingUpdateable(order, "main-last", updateLast: true));
        world.Add(new OrderTrackingUpdateable(order, "main-regular"));
        world.Overlay.Add(new OrderTrackingUpdateable(order, "overlay-last", updateLast: true));
        world.Overlay.Add(new OrderTrackingUpdateable(order, "overlay-regular"));

        world.Update(0.016);

        // Contract: each container completes its own regular-then-last pass before the next
        // container starts. So main-regular → main-last → overlay-regular → overlay-last.
        Assert.HasCount(4, order);
        Assert.AreEqual("main-regular", order[0]);
        Assert.AreEqual("main-last", order[1]);
        Assert.AreEqual("overlay-regular", order[2]);
        Assert.AreEqual("overlay-last", order[3]);
    }

    [TestMethod]
    public void Update_InactiveUpdateLast_IsSkipped()
    {
        var world = new World();
        var order = new List<string>();
        var last = new OrderTrackingUpdateable(order, "last", updateLast: true) { Active = false };
        world.Add(new OrderTrackingUpdateable(order, "regular"));
        world.Add(last);

        world.Update(0.016);

        Assert.HasCount(1, order);
        Assert.AreEqual("regular", order[0]);
    }

    // Unload — must dispose IDisposable children so GL resources don't leak on world swap.

    private sealed class DisposableRenderable : AddableBase, IRenderable, IDisposable
    {
        public bool Visible { get; set; } = true;
        public int DisposeCallCount { get; private set; }
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
        public void Dispose() => DisposeCallCount++;
    }

    private sealed class DisposableUpdateable : AddableBase, IUpdateable, IDisposable
    {
        public bool Active { get; set; } = true;
        public int DisposeCallCount { get; private set; }
        public void Update(double elapsed) { }
        public void Dispose() => DisposeCallCount++;
    }

    [TestMethod]
    public void Unload_DisposesDisposableRenderableChildren()
    {
        var world = new World();
        var graphic = new DisposableRenderable();
        world.Add(graphic);

        world.Unload();

        Assert.AreEqual(1, graphic.DisposeCallCount);
    }

    [TestMethod]
    public void Unload_DisposesDisposableUpdateableChildren()
    {
        var world = new World();
        var ticker = new DisposableUpdateable();
        world.Add(ticker);

        world.Unload();

        Assert.AreEqual(1, ticker.DisposeCallCount);
    }

    [TestMethod]
    public void Unload_DisposesOverlayChildren()
    {
        var world = new World();
        var overlayGraphic = new DisposableRenderable();
        world.Overlay.Add(overlayGraphic);

        world.Unload();

        Assert.AreEqual(1, overlayGraphic.DisposeCallCount);
    }

    [TestMethod]
    public void Unload_RemovesAllChildrenSoTheirParentIsCleared()
    {
        var world = new World();
        var graphic = new DisposableRenderable();
        var ticker = new DisposableUpdateable();
        world.Add(graphic);
        world.Add(ticker);

        world.Unload();

        Assert.IsNull(graphic.Parent);
        Assert.IsNull(ticker.Parent);
    }
}
