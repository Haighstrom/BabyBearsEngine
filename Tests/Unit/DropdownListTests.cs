using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class DropdownListTests
{
    /// <summary>Stub entity that can simulate a click without a ClickController or OpenGL.</summary>
    private sealed class StubEntity : Entity
    {
        internal string LastSetText { get; private set; } = "";

        internal StubEntity() : base(0f, 0f, 200f, 40f) { }

        internal void SimulateClick() => OnLeftClicked();

        internal void SetText(string text) => LastSetText = text;
    }

    // Test root that gives the dropdown a parent so its GetWindowCoordinates works.
    // GetWindowCoordinates returns the input unchanged — same as a World at the tree root.
    private sealed class FakeRoot : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private sealed class TestFixture
    {
        internal DropdownList<string> Dropdown { get; }
        internal StubEntity Header { get; }
        internal List<StubEntity> Options { get; } = [];

        internal TestFixture(string[]? items = null, int initialIndex = 0)
        {
            items ??= ["Alpha", "Beta", "Gamma"];
            Header = new StubEntity();
            Dropdown = new DropdownList<string>(
                x: 0f, y: 0f, width: 200f, height: 40f,
                items: items,
                header: Header,
                setHeaderText: Header.SetText,
                optionFactory: (i, _) =>
                {
                    StubEntity option = new();
                    Options.Add(option);
                    return option;
                },
                formatter: null,
                initialIndex: initialIndex);
            Dropdown.Parent = new FakeRoot();
        }
    }

    // Constructor

    [TestMethod]
    public void Constructor_SetsCurrentValue()
    {
        TestFixture f = new();

        Assert.AreEqual("Alpha", f.Dropdown.CurrentValue);
    }

    [TestMethod]
    public void Constructor_RespectsInitialIndex()
    {
        TestFixture f = new(initialIndex: 2);

        Assert.AreEqual("Gamma", f.Dropdown.CurrentValue);
        Assert.AreEqual(2, f.Dropdown.CurrentIndex);
    }

    [TestMethod]
    public void Constructor_IsClosedInitially()
    {
        TestFixture f = new();

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void Constructor_EmptyItems_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestFixture(items: []));
    }

    // Open

    [TestMethod]
    public void Open_SetsIsOpen()
    {
        TestFixture f = new();

        f.Dropdown.Open();

        Assert.IsTrue(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void Open_WhenAlreadyOpen_IsNoOp()
    {
        TestFixture f = new();
        f.Dropdown.Open();

        f.Dropdown.Open();

        Assert.IsTrue(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void Open_OptionListIsClickBlocking()
    {
        // The option list must be clickable so clicks landing in the gaps between option rows are
        // absorbed instead of falling through to whatever sits behind the open dropdown.
        TestFixture f = new();

        f.Dropdown.Open();

        Assert.IsNotNull(f.Dropdown.OpenOptionList);
        Assert.IsTrue(f.Dropdown.OpenOptionList.IsClickable);
    }

    [TestMethod]
    public void Open_WhenNotParented_ThrowsClearerExceptionMentioningParent()
    {
        StubEntity header = new();
        DropdownList<string> dropdown = new(
            x: 0f, y: 0f, width: 200f, height: 40f,
            items: ["A"],
            header: header,
            setHeaderText: header.SetText,
            optionFactory: (_, _) => new StubEntity(),
            formatter: null,
            initialIndex: 0);
        // Note: not adding to a parent.

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => dropdown.Open());
        // Message should point at the caller's API (Open / DropdownList), not the internal
        // GetWindowCoordinates helper that actually trips first.
        Assert.IsTrue(
            ex.Message.Contains("Open", System.StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("DropdownList", System.StringComparison.OrdinalIgnoreCase),
            $"Expected exception message to mention Open or DropdownList, got: {ex.Message}");
    }

    // Close

    [TestMethod]
    public void Close_SetsIsOpenFalse()
    {
        TestFixture f = new();
        f.Dropdown.Open();

        f.Dropdown.Close();

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void Close_WhenAlreadyClosed_DoesNotThrow()
    {
        TestFixture f = new();

        f.Dropdown.Close();

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void RemovedFromParent_WhileOpen_ForcesClose()
    {
        TestFixture f = new();
        f.Dropdown.Open();
        Assert.IsTrue(f.Dropdown.IsOpen);

        f.Dropdown.Parent = null;

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void RemovedFromParent_WhileOpen_OptionClickAfterwardsDoesNotFireSelectionChanged()
    {
        // Initial index 0 ("Alpha"); click option 1 ("Beta") would normally fire SelectionChanged.
        TestFixture f = new();
        f.Dropdown.Open();
        StubEntity secondOption = f.Options[1];
        bool fired = false;
        f.Dropdown.SelectionChanged += (_, _) => fired = true;

        f.Dropdown.Parent = null;
        secondOption.SimulateClick();

        Assert.IsFalse(fired);
    }

    // SelectItem

    [TestMethod]
    public void SelectItem_ChangesCurrentValue()
    {
        TestFixture f = new();

        f.Dropdown.SelectItem(2);

        Assert.AreEqual("Gamma", f.Dropdown.CurrentValue);
    }

    [TestMethod]
    public void SelectItem_ChangesCurrentIndex()
    {
        TestFixture f = new();

        f.Dropdown.SelectItem(1);

        Assert.AreEqual(1, f.Dropdown.CurrentIndex);
    }

    [TestMethod]
    public void SelectItem_UpdatesHeaderText()
    {
        TestFixture f = new();

        f.Dropdown.SelectItem(2);

        Assert.AreEqual("Gamma", f.Header.LastSetText);
    }

    [TestMethod]
    public void SelectItem_ClosesDropdown()
    {
        TestFixture f = new();
        f.Dropdown.Open();

        f.Dropdown.SelectItem(1);

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void SelectItem_RaisesSelectionChangedWithCorrectArgs()
    {
        TestFixture f = new();
        SelectionChangedEventArgs<string>? captured = null;
        f.Dropdown.SelectionChanged += (_, e) => captured = e;

        f.Dropdown.SelectItem(2);

        Assert.IsNotNull(captured);
        Assert.AreEqual("Alpha", captured.OldValue);
        Assert.AreEqual("Gamma", captured.NewValue);
    }

    [TestMethod]
    public void SelectItem_SameIndex_DoesNotRaiseSelectionChanged()
    {
        TestFixture f = new();
        int raisedCount = 0;
        f.Dropdown.SelectionChanged += (_, _) => raisedCount++;

        f.Dropdown.SelectItem(0);

        Assert.AreEqual(0, raisedCount);
    }

    // Option click wiring

    [TestMethod]
    public void Open_WhenOptionClicked_SelectsCorrectItem()
    {
        TestFixture f = new();
        f.Dropdown.Open();

        f.Options[1].SimulateClick();

        Assert.AreEqual("Beta", f.Dropdown.CurrentValue);
    }

    [TestMethod]
    public void Open_WhenOptionClicked_ClosesDropdown()
    {
        TestFixture f = new();
        f.Dropdown.Open();

        f.Options[0].SimulateClick();

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    // Header click wiring

    [TestMethod]
    public void HeaderClick_OpensDropdown()
    {
        TestFixture f = new();

        f.Header.SimulateClick();

        Assert.IsTrue(f.Dropdown.IsOpen);
    }

    [TestMethod]
    public void HeaderClick_WhenOpen_ClosesDropdown()
    {
        TestFixture f = new();
        f.Dropdown.Open();

        f.Header.SimulateClick();

        Assert.IsFalse(f.Dropdown.IsOpen);
    }

    // CalculateListLocalY

    [TestMethod]
    public void CalculateListLocalY_FitsBelow_ReturnsHeaderHeight()
    {
        float result = DropdownList<string>.CalculateListLocalY(
            headerHeight: 40f, listHeight: 120f, headerBottomWindowY: 200f, viewportHeight: 600f);

        Assert.AreEqual(40f, result);
    }

    [TestMethod]
    public void CalculateListLocalY_ExactlyFits_ReturnsHeaderHeight()
    {
        float result = DropdownList<string>.CalculateListLocalY(
            headerHeight: 40f, listHeight: 120f, headerBottomWindowY: 480f, viewportHeight: 600f);

        Assert.AreEqual(40f, result);
    }

    [TestMethod]
    public void CalculateListLocalY_DoesNotFitBelow_ReturnsNegativeListHeight()
    {
        float result = DropdownList<string>.CalculateListLocalY(
            headerHeight: 40f, listHeight: 120f, headerBottomWindowY: 500f, viewportHeight: 600f);

        Assert.AreEqual(-120f, result);
    }
}
