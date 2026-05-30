using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TabbedPanelTests
{
    private sealed class StubGraphic : GraphicBase, IGraphic
    {
        public Colour Colour { get; set; }
        public float Angle { get; set; } = 0f;
        public override void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private static TabbedPanelTheme StubTheme() => new()
    {
        PanelBackgroundFactory = _ => new StubGraphic(),
        ActiveTabFactory = _ => new StubGraphic(),
        InactiveTabFactory = _ => new StubGraphic(),
        TabText = TextTheme.Default,
    };

    private static TabbedPanel MakePanel() =>
        new(0, 0, 400, 300, 40, StubTheme());

    private static Tab MakeTab(float width = 80f, float height = 30f) =>
        new(width, height, new StubGraphic(), new StubGraphic());

    // Initial state

    [TestMethod]
    public void Constructor_CurrentTabIsNull()
    {
        TabbedPanel panel = MakePanel();

        Assert.IsNull(panel.CurrentTab);
    }

    [TestMethod]
    public void Constructor_TabsIsEmpty()
    {
        TabbedPanel panel = MakePanel();

        Assert.IsEmpty(panel.Tabs);
    }

    // AddTab

    [TestMethod]
    public void AddTab_FirstTab_BecomesCurrentTab()
    {
        TabbedPanel panel = MakePanel();
        Tab tab = MakeTab();

        panel.AddTab(tab);

        Assert.AreEqual(tab, panel.CurrentTab);
    }

    [TestMethod]
    public void AddTab_FirstTab_RaisesTabChanged()
    {
        TabbedPanel panel = MakePanel();
        Tab tab = MakeTab();
        TabChangedEventArgs? args = null;
        panel.TabChanged += (_, e) => args = e;

        panel.AddTab(tab);

        Assert.IsNotNull(args);
        Assert.IsNull(args.Previous);
        Assert.AreEqual(tab, args.Current);
    }

    [TestMethod]
    public void AddTab_SecondTab_DoesNotChangeCurrent()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);

        panel.AddTab(second);

        Assert.AreEqual(first, panel.CurrentTab);
    }

    [TestMethod]
    public void AddTab_SecondTab_IsInTabs()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);

        panel.AddTab(second);

        Assert.HasCount(2, panel.Tabs);
        Assert.Contains(second, panel.Tabs);
    }

    [TestMethod]
    public void AddTab_WithActivateTrue_SwitchesToNewTab()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);

        panel.AddTab(second, activate: true);

        Assert.AreEqual(second, panel.CurrentTab);
    }

    [TestMethod]
    public void AddTab_WithActivateTrue_RaisesTabChanged()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);
        int raised = 0;
        panel.TabChanged += (_, _) => raised++;

        panel.AddTab(second, activate: true);

        Assert.AreEqual(1, raised);
    }

    // SwitchTo

    [TestMethod]
    public void SwitchTo_ChangesCurrentTab()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);
        panel.AddTab(second);

        panel.SwitchTo(second);

        Assert.AreEqual(second, panel.CurrentTab);
    }

    [TestMethod]
    public void SwitchTo_RaisesTabChanged_WithCorrectArgs()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);
        panel.AddTab(second);
        TabChangedEventArgs? args = null;
        panel.TabChanged += (_, e) => args = e;

        panel.SwitchTo(second);

        Assert.IsNotNull(args);
        Assert.AreEqual(first, args.Previous);
        Assert.AreEqual(second, args.Current);
    }

    [TestMethod]
    public void SwitchTo_AlreadyActiveTab_DoesNotRaiseTabChanged()
    {
        TabbedPanel panel = MakePanel();
        Tab tab = MakeTab();
        panel.AddTab(tab);
        int raised = 0;
        panel.TabChanged += (_, _) => raised++;

        panel.SwitchTo(tab);

        Assert.AreEqual(0, raised);
    }

    [TestMethod]
    public void SwitchTo_TabNotInPanel_ThrowsArgumentException()
    {
        TabbedPanel panel = MakePanel();
        Tab outsider = MakeTab();

        Assert.ThrowsExactly<ArgumentException>(() => panel.SwitchTo(outsider));
    }

    // RemoveTab

    [TestMethod]
    public void RemoveTab_RemovesFromTabsCollection()
    {
        TabbedPanel panel = MakePanel();
        Tab tab = MakeTab();
        panel.AddTab(tab);

        panel.RemoveTab(tab);

        Assert.IsEmpty(panel.Tabs);
    }

    [TestMethod]
    public void RemoveTab_InactiveTab_CurrentTabUnchanged()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);
        panel.AddTab(second);

        panel.RemoveTab(second);

        Assert.AreEqual(first, panel.CurrentTab);
    }

    [TestMethod]
    public void RemoveTab_ActiveTab_SwitchesToAdjacentTab()
    {
        TabbedPanel panel = MakePanel();
        Tab first = MakeTab();
        Tab second = MakeTab();
        panel.AddTab(first);
        panel.AddTab(second);
        panel.SwitchTo(first);

        panel.RemoveTab(first);

        Assert.AreEqual(second, panel.CurrentTab);
    }

    [TestMethod]
    public void RemoveTab_LastTab_CurrentTabBecomesNull()
    {
        TabbedPanel panel = MakePanel();
        Tab tab = MakeTab();
        panel.AddTab(tab);

        panel.RemoveTab(tab);

        Assert.IsNull(panel.CurrentTab);
    }

    [TestMethod]
    public void RemoveTab_TabNotInPanel_DoesNothing()
    {
        TabbedPanel panel = MakePanel();
        Tab tab = MakeTab();
        panel.AddTab(tab);
        Tab outsider = MakeTab();

        panel.RemoveTab(outsider);

        Assert.HasCount(1, panel.Tabs);
    }
}
