using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class DraggablePanelTests
{
    [TestMethod]
    public void GrabBarHeight_DefaultsTo30()
    {
        var panel = new DraggablePanel(0f, 0f, 100f, 60f);

        Assert.AreEqual(30f, panel.GrabBarHeight);
    }

    [TestMethod]
    public void GrabBarHeight_ConstructorOverride_IsHonoured()
    {
        var panel = new DraggablePanel(0f, 0f, 100f, 60f, grabBarHeight: 45f);

        Assert.AreEqual(45f, panel.GrabBarHeight);
    }

    [TestMethod]
    public void GrabBarHeight_IsSettableAtRuntime()
    {
        var panel = new DraggablePanel(0f, 0f, 100f, 60f);

        panel.GrabBarHeight = 50f;

        Assert.AreEqual(50f, panel.GrabBarHeight);
    }
}
