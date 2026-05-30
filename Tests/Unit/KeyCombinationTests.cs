using System;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class KeyCombinationTests
{
    // Construction & defaults

    [TestMethod]
    public void DefaultModifiers_IsNone()
    {
        KeyCombination combination = new(Keys.A);
        Assert.AreEqual(KeyModifiers.None, combination.Modifiers);
        Assert.AreEqual(Keys.A, combination.Key);
    }

    // Equality (free from record struct, but lock the contract in)

    [TestMethod]
    public void Equality_SameKeyAndModifiers_AreEqual()
    {
        KeyCombination a = new(Keys.S, KeyModifiers.Ctrl);
        KeyCombination b = new(Keys.S, KeyModifiers.Ctrl);
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_DifferentModifiers_AreNotEqual()
    {
        Assert.AreNotEqual(new KeyCombination(Keys.S, KeyModifiers.Ctrl), new KeyCombination(Keys.S, KeyModifiers.Shift));
    }

    [TestMethod]
    public void Equality_DifferentKeys_AreNotEqual()
    {
        Assert.AreNotEqual(new KeyCombination(Keys.S, KeyModifiers.Ctrl), new KeyCombination(Keys.A, KeyModifiers.Ctrl));
    }

    // ToString

    [TestMethod]
    public void ToString_NoModifiers_ReturnsKeyName()
    {
        Assert.AreEqual("S", new KeyCombination(Keys.S).ToString());
    }

    [TestMethod]
    public void ToString_SingleModifier_FormatsAsModifierPlusKey()
    {
        Assert.AreEqual("Ctrl+S", new KeyCombination(Keys.S, KeyModifiers.Ctrl).ToString());
        Assert.AreEqual("Shift+A", new KeyCombination(Keys.A, KeyModifiers.Shift).ToString());
        Assert.AreEqual("Alt+F4", new KeyCombination(Keys.F4, KeyModifiers.Alt).ToString());
    }

    [TestMethod]
    public void ToString_MultipleModifiers_AppearInFixedOrderCtrlShiftAlt()
    {
        KeyCombination combination = new(Keys.Z, KeyModifiers.Shift | KeyModifiers.Ctrl);
        Assert.AreEqual("Ctrl+Shift+Z", combination.ToString());

        KeyCombination all = new(Keys.Delete, KeyModifiers.Alt | KeyModifiers.Ctrl | KeyModifiers.Shift);
        Assert.AreEqual("Ctrl+Shift+Alt+Delete", all.ToString());
    }

    // TryParse — success cases

    [TestMethod]
    public void TryParse_SingleKey_Succeeds()
    {
        Assert.IsTrue(KeyCombination.TryParse("Enter", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(Keys.Enter), combination);
    }

    [TestMethod]
    public void TryParse_WithCtrl_Succeeds()
    {
        Assert.IsTrue(KeyCombination.TryParse("Ctrl+S", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(Keys.S, KeyModifiers.Ctrl), combination);
    }

    [TestMethod]
    public void TryParse_ControlSpelledOut_IsAcceptedAsCtrl()
    {
        Assert.IsTrue(KeyCombination.TryParse("Control+S", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(Keys.S, KeyModifiers.Ctrl), combination);
    }

    [TestMethod]
    public void TryParse_CaseInsensitive()
    {
        Assert.IsTrue(KeyCombination.TryParse("ctrl+shift+z", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(Keys.Z, KeyModifiers.Ctrl | KeyModifiers.Shift), combination);
    }

    [TestMethod]
    public void TryParse_WhitespaceAroundSeparators_IsTolerated()
    {
        Assert.IsTrue(KeyCombination.TryParse(" Ctrl + Shift + S ", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(Keys.S, KeyModifiers.Ctrl | KeyModifiers.Shift), combination);
    }

    // TryParse — failure cases

    [TestMethod]
    public void TryParse_NullOrEmpty_ReturnsFalse()
    {
        Assert.IsFalse(KeyCombination.TryParse(null, out _));
        Assert.IsFalse(KeyCombination.TryParse("", out _));
        Assert.IsFalse(KeyCombination.TryParse("   ", out _));
    }

    [TestMethod]
    public void TryParse_UnknownKeyName_ReturnsFalse()
    {
        Assert.IsFalse(KeyCombination.TryParse("Ctrl+Banana", out _));
    }

    [TestMethod]
    public void TryParse_UnknownModifier_ReturnsFalse()
    {
        Assert.IsFalse(KeyCombination.TryParse("Win+S", out _));
    }

    [TestMethod]
    public void TryParse_NumericKey_RejectedAsKeyName()
    {
        Assert.IsFalse(KeyCombination.TryParse("Ctrl+999", out _));
    }

    // Round-trip

    [TestMethod]
    public void ToStringThenParse_RoundTrips()
    {
        KeyCombination[] cases =
        [
            new(Keys.S),
            new(Keys.S, KeyModifiers.Ctrl),
            new(Keys.Z, KeyModifiers.Ctrl | KeyModifiers.Shift),
            new(Keys.F4, KeyModifiers.Alt),
            new(Keys.Delete, KeyModifiers.Ctrl | KeyModifiers.Shift | KeyModifiers.Alt),
            new(Keys.Space),
            new(Keys.D7, KeyModifiers.Ctrl),
        ];

        foreach (KeyCombination original in cases)
        {
            string rendered = original.ToString();
            Assert.IsTrue(KeyCombination.TryParse(rendered, out KeyCombination roundTripped), $"Failed to parse '{rendered}'.");
            Assert.AreEqual(original, roundTripped, $"Round-trip mismatch for '{rendered}'.");
        }
    }

    // Parse

    [TestMethod]
    public void Parse_ValidInput_ReturnsCombination()
    {
        Assert.AreEqual(new KeyCombination(Keys.S, KeyModifiers.Ctrl), KeyCombination.Parse("Ctrl+S"));
    }

    [TestMethod]
    public void Parse_InvalidInput_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => KeyCombination.Parse("not a combo"));
    }

    [TestMethod]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => KeyCombination.Parse(null!));
    }
}
