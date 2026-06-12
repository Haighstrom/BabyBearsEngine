using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ButtonTests
{
    private sealed class TestButton : Button
    {
        internal TestButton() : base(0, 0, 100, 30) { }
        internal void FireLeftClicked() => OnLeftClicked();
        internal void FireLeftPressed() => OnLeftPressed();
        internal void FireMouseEntered() => OnMouseEntered();
        internal void FireMouseExited() => OnMouseExited();
    }

    // Text

    [TestMethod]
    public void Text_DefaultsToEmpty()
    {
        TestButton button = new();

        Assert.AreEqual(string.Empty, button.Text);
    }

    [TestMethod]
    public void Text_Set_WithoutTextGraphic_Throws()
    {
        // The no-text test ctor leaves the text graphic null; setting Text must fail loudly rather
        // than silently discard the assignment.
        TestButton button = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => button.Text = "Hello");
    }

    [TestMethod]
    public void Text_Set_WithTextGraphic_RoundTrips()
    {
        Button button = new(0, 0, 100, 30, new StubTextGraphic());

        button.Text = "Hello";

        Assert.AreEqual("Hello", button.Text);
    }

    // Events

    [TestMethod]
    public void LeftClicked_FiresWhenOnLeftClickedCalled()
    {
        TestButton button = new();
        int count = 0;
        button.LeftClicked += (_, _) => count++;

        button.FireLeftClicked();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void LeftPressed_FiresWhenOnLeftPressedCalled()
    {
        TestButton button = new();
        int count = 0;
        button.LeftPressed += (_, _) => count++;

        button.FireLeftPressed();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void MouseEntered_FiresWhenOnMouseEnteredCalled()
    {
        TestButton button = new();
        int count = 0;
        button.MouseEntered += (_, _) => count++;

        button.FireMouseEntered();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void MouseExited_FiresWhenOnMouseExitedCalled()
    {
        TestButton button = new();
        int count = 0;
        button.MouseExited += (_, _) => count++;

        button.FireMouseExited();

        Assert.AreEqual(1, count);
    }

    // State changes with null theme — must not throw

    [TestMethod]
    public void InteractionSequence_WithNullTheme_DoesNotThrow()
    {
        TestButton button = new();

        button.FireMouseEntered();
        button.FireLeftPressed();
        button.FireLeftClicked();
        button.FireMouseExited();
    }
}
