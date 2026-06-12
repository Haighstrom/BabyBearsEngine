using System.Collections.Generic;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CyclingValueButtonTests
{
    private sealed class TestCyclingButton<T> : CyclingValueButton<T>
    {
        internal TestCyclingButton(IReadOnlyList<T> values, int initialIndex = 0)
            : base(values, new StubTextGraphic(), initialIndex) { }
        internal void FireLeftClicked() => OnLeftClicked();
    }

    private static TestCyclingButton<string> MakeButton(params string[] values) =>
        new(values);

    // Initial state

    [TestMethod]
    public void Constructor_CurrentValueIsFirstElement()
    {
        TestCyclingButton<string> button = MakeButton("A", "B", "C");

        Assert.AreEqual("A", button.CurrentValue);
    }

    [TestMethod]
    public void Constructor_CurrentIndexIsZero()
    {
        TestCyclingButton<string> button = MakeButton("A", "B", "C");

        Assert.AreEqual(0, button.CurrentIndex);
    }

    [TestMethod]
    public void Constructor_InitialIndex_SetsCurrentValue()
    {
        TestCyclingButton<string> button = new(["A", "B", "C"], initialIndex: 2);

        Assert.AreEqual("C", button.CurrentValue);
        Assert.AreEqual(2, button.CurrentIndex);
    }

    // Cycling

    [TestMethod]
    public void Click_AdvancesToNextValue()
    {
        TestCyclingButton<string> button = MakeButton("A", "B", "C");

        button.FireLeftClicked();

        Assert.AreEqual("B", button.CurrentValue);
    }

    [TestMethod]
    public void Click_MultipleTimes_AdvancesThroughValues()
    {
        TestCyclingButton<string> button = MakeButton("A", "B", "C");

        button.FireLeftClicked();
        button.FireLeftClicked();

        Assert.AreEqual("C", button.CurrentValue);
    }

    [TestMethod]
    public void Click_AtLastElement_WrapsToFirst()
    {
        TestCyclingButton<string> button = new(["A", "B", "C"], initialIndex: 2);

        button.FireLeftClicked();

        Assert.AreEqual("A", button.CurrentValue);
        Assert.AreEqual(0, button.CurrentIndex);
    }

    // ValueChanged event

    [TestMethod]
    public void Click_RaisesValueChangedEvent()
    {
        TestCyclingButton<string> button = MakeButton("A", "B");
        bool raised = false;
        button.ValueChanged += (_, _) => raised = true;

        button.FireLeftClicked();

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void Click_ValueChangedEventArgs_HasCorrectOldAndNewValues()
    {
        TestCyclingButton<string> button = MakeButton("A", "B", "C");
        CyclingValueChangedEventArgs<string>? args = null;
        button.ValueChanged += (_, e) => args = e;

        button.FireLeftClicked();

        Assert.IsNotNull(args);
        Assert.AreEqual("A", args.OldValue);
        Assert.AreEqual("B", args.NewValue);
    }

    [TestMethod]
    public void Click_WrapAround_ValueChangedEventArgs_HasCorrectValues()
    {
        TestCyclingButton<string> button = new(["X", "Y"], initialIndex: 1);
        CyclingValueChangedEventArgs<string>? args = null;
        button.ValueChanged += (_, e) => args = e;

        button.FireLeftClicked();

        Assert.IsNotNull(args);
        Assert.AreEqual("Y", args.OldValue);
        Assert.AreEqual("X", args.NewValue);
    }

    // Single-element list

    [TestMethod]
    public void SingleElement_Click_StaysOnSameValue()
    {
        TestCyclingButton<int> button = new([42]);

        button.FireLeftClicked();

        Assert.AreEqual(42, button.CurrentValue);
        Assert.AreEqual(0, button.CurrentIndex);
    }
}
