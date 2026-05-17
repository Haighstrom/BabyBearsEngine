using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SimpleToolTipTests
{
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
}
