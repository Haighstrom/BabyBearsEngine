using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SimpleToolTipTests
{
    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) => entity.Parent = null;
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    // Test seam that exposes Entity's protected hover-event raisers, so tests can drive the
    // attached tooltip without spinning up a real click controller and mouse-input pipeline.
    private sealed class TestTarget(float x, float y, float w, float h) : Entity(x, y, w, h)
    {
        public void RaiseMouseHovered() => OnMouseHovered();
        public void RaiseMouseHoverStopped() => OnStopMouseHovered();
        public void RaiseMouseExited() => OnMouseExited();
    }

    private static SimpleToolTip Make() => new(0, 0, 120, 30);

    // Initial state

    [TestMethod]
    public void Constructor_VisibleIsFalse()
    {
        SimpleToolTip tip = Make();

        Assert.IsFalse(tip.Visible);
    }

    [TestMethod]
    public void Constructor_TextIsEmpty()
    {
        SimpleToolTip tip = Make();

        Assert.AreEqual(string.Empty, tip.Text);
    }

    // Show / Hide

    [TestMethod]
    public void Show_SetsVisibleTrue()
    {
        SimpleToolTip tip = Make();

        tip.Show();

        Assert.IsTrue(tip.Visible);
    }

    [TestMethod]
    public void Hide_SetsVisibleFalse()
    {
        SimpleToolTip tip = Make();
        tip.Show();

        tip.Hide();

        Assert.IsFalse(tip.Visible);
    }

    [TestMethod]
    public void Show_ThenHide_ThenShow_IsVisible()
    {
        SimpleToolTip tip = Make();

        tip.Show();
        tip.Hide();
        tip.Show();

        Assert.IsTrue(tip.Visible);
    }

    // Text property

    [TestMethod]
    public void Text_Set_IsNoOpWithoutTextGraphic()
    {
        SimpleToolTip tip = Make();

        tip.Text = "Hello";

        Assert.AreEqual(string.Empty, tip.Text);
    }

    // AttachTo

    [TestMethod]
    public void AttachTo_TargetHovered_TooltipBecomesVisible()
    {
        SimpleToolTip tip = Make();
        var target = new TestTarget(0, 0, 50, 50);
        tip.AttachTo(target);

        target.RaiseMouseHovered();

        Assert.IsTrue(tip.Visible);
    }

    [TestMethod]
    public void AttachTo_TargetHoverStopped_TooltipBecomesHidden()
    {
        SimpleToolTip tip = Make();
        var target = new TestTarget(0, 0, 50, 50);
        tip.AttachTo(target);
        target.RaiseMouseHovered();

        target.RaiseMouseHoverStopped();

        Assert.IsFalse(tip.Visible);
    }

    [TestMethod]
    public void AttachTo_TargetExited_TooltipBecomesHidden()
    {
        SimpleToolTip tip = Make();
        var target = new TestTarget(0, 0, 50, 50);
        tip.AttachTo(target);
        target.RaiseMouseHovered();

        target.RaiseMouseExited();

        Assert.IsFalse(tip.Visible);
    }

    [TestMethod]
    public void AttachTo_TargetRemovedFromTreeMidHover_TooltipBecomesHidden()
    {
        SimpleToolTip tip = Make();
        var target = new TestTarget(0, 0, 50, 50);
        var container = new FakeContainer();
        target.Parent = container;
        tip.AttachTo(target);
        target.RaiseMouseHovered();
        Assert.IsTrue(tip.Visible);

        // Caller removes the target mid-hover (e.g. a settings row is regenerated).
        target.Parent = null;

        Assert.IsFalse(tip.Visible);
    }

    [TestMethod]
    public void AttachTo_AfterTargetRemoved_HoverEventsAreNotPropagated()
    {
        SimpleToolTip tip = Make();
        var target = new TestTarget(0, 0, 50, 50);
        target.Parent = new FakeContainer();
        tip.AttachTo(target);
        target.Parent = null;

        target.RaiseMouseHovered();

        // Unsubscribed when target was removed; stray events shouldn't re-show the tooltip.
        Assert.IsFalse(tip.Visible);
    }
}
