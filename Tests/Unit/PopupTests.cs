using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PopupTests
{
    private sealed class TestPopup : Popup
    {
        internal TestPopup() : base(0, 0, 100, 100) { }
    }

    // Defaults

    [TestMethod]
    public void Defaults_IsClosedAndHidden()
    {
        TestPopup popup = new();

        Assert.IsFalse(popup.IsOpen);
        Assert.IsFalse(popup.Visible);
        Assert.IsFalse(popup.Active);
    }

    // Open

    [TestMethod]
    public void Open_SetsIsOpenVisibleActive()
    {
        TestPopup popup = new();

        popup.Open();

        Assert.IsTrue(popup.IsOpen);
        Assert.IsTrue(popup.Visible);
        Assert.IsTrue(popup.Active);
    }

    // Close

    [TestMethod]
    public void Close_ClearsIsOpenVisibleActive()
    {
        TestPopup popup = new();
        popup.Open();

        popup.Close();

        Assert.IsFalse(popup.IsOpen);
        Assert.IsFalse(popup.Visible);
        Assert.IsFalse(popup.Active);
    }

    [TestMethod]
    public void Close_WhenAlreadyClosed_IsNoOp()
    {
        TestPopup popup = new();

        popup.Close();

        Assert.IsFalse(popup.IsOpen);
    }
}
