using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CheckboxTests
{
    private sealed class TestCheckbox : Checkbox
    {
        internal TestCheckbox(bool isChecked = false) : base(0, 0, 30, 30, isChecked) { }
        internal void FireLeftClicked() => OnLeftClicked();
    }

    // Initial state

    [TestMethod]
    public void Constructor_DefaultIsCheckedFalse()
    {
        TestCheckbox cb = new();

        Assert.IsFalse(cb.IsChecked);
    }

    [TestMethod]
    public void Constructor_IsCheckedTrue_InitialisesChecked()
    {
        TestCheckbox cb = new(isChecked: true);

        Assert.IsTrue(cb.IsChecked);
    }

    // Direct assignment — Checked event

    [TestMethod]
    public void IsChecked_SetTrue_RaisesCheckedEvent()
    {
        TestCheckbox cb = new();
        int raised = 0;
        cb.Checked += (_, _) => raised++;

        cb.IsChecked = true;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void IsChecked_SetFalse_RaisesUncheckedEvent()
    {
        TestCheckbox cb = new(isChecked: true);
        int raised = 0;
        cb.Unchecked += (_, _) => raised++;

        cb.IsChecked = false;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void IsChecked_SetSameValue_DoesNotRaiseEvents()
    {
        TestCheckbox cb = new();
        int raised = 0;
        cb.Checked += (_, _) => raised++;
        cb.Unchecked += (_, _) => raised++;

        cb.IsChecked = false;

        Assert.AreEqual(0, raised);
    }

    // Click toggles state

    [TestMethod]
    public void Click_TogglesIsCheckedFromFalseToTrue()
    {
        TestCheckbox cb = new();

        cb.FireLeftClicked();

        Assert.IsTrue(cb.IsChecked);
    }

    [TestMethod]
    public void Click_TogglesIsCheckedFromTrueToFalse()
    {
        TestCheckbox cb = new(isChecked: true);

        cb.FireLeftClicked();

        Assert.IsFalse(cb.IsChecked);
    }

    [TestMethod]
    public void Click_RaisesCheckedEvent()
    {
        TestCheckbox cb = new();
        int raised = 0;
        cb.Checked += (_, _) => raised++;

        cb.FireLeftClicked();

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Click_DoesNotThrow_WithNullTick()
    {
        TestCheckbox cb = new();

        cb.FireLeftClicked();
        cb.FireLeftClicked();
    }
}
