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
        KeyCombination combination = new(KeyModifiers.None, Keys.A);
        Assert.AreEqual(KeyModifiers.None, combination.Modifiers);
        Assert.AreEqual(Keys.A, combination.Key);
    }

    // Equality (free from record struct, but lock the contract in)

    [TestMethod]
    public void Equality_SameKeyAndModifiers_AreEqual()
    {
        KeyCombination a = new(KeyModifiers.Ctrl, Keys.S);
        KeyCombination b = new(KeyModifiers.Ctrl, Keys.S);
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_DifferentModifiers_AreNotEqual()
    {
        Assert.AreNotEqual(new KeyCombination(KeyModifiers.Ctrl, Keys.S), new KeyCombination(KeyModifiers.Shift, Keys.S));
    }

    [TestMethod]
    public void Equality_DifferentKeys_AreNotEqual()
    {
        Assert.AreNotEqual(new KeyCombination(KeyModifiers.Ctrl, Keys.S), new KeyCombination(KeyModifiers.Ctrl, Keys.A));
    }

    // ToString

    [TestMethod]
    public void ToString_NoModifiers_ReturnsKeyName()
    {
        Assert.AreEqual("S", new KeyCombination(KeyModifiers.None, Keys.S).ToString());
    }

    [TestMethod]
    public void ToString_SingleModifier_FormatsAsModifierPlusKey()
    {
        Assert.AreEqual("Ctrl+S", new KeyCombination(KeyModifiers.Ctrl, Keys.S).ToString());
        Assert.AreEqual("Shift+A", new KeyCombination(KeyModifiers.Shift, Keys.A).ToString());
        Assert.AreEqual("Alt+F4", new KeyCombination(KeyModifiers.Alt, Keys.F4).ToString());
    }

    [TestMethod]
    public void ToString_MultipleModifiers_AppearInFixedOrderCtrlShiftAlt()
    {
        KeyCombination combination = new(KeyModifiers.Shift | KeyModifiers.Ctrl, Keys.Z);
        Assert.AreEqual("Ctrl+Shift+Z", combination.ToString());

        KeyCombination all = new(KeyModifiers.Alt | KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.Delete);
        Assert.AreEqual("Ctrl+Shift+Alt+Delete", all.ToString());
    }

    // TryParse — success cases

    [TestMethod]
    public void TryParse_SingleKey_Succeeds()
    {
        Assert.IsTrue(KeyCombination.TryParse("Enter", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(KeyModifiers.None, Keys.Enter), combination);
    }

    [TestMethod]
    public void TryParse_WithCtrl_Succeeds()
    {
        Assert.IsTrue(KeyCombination.TryParse("Ctrl+S", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(KeyModifiers.Ctrl, Keys.S), combination);
    }

    [TestMethod]
    public void TryParse_ControlSpelledOut_IsAcceptedAsCtrl()
    {
        Assert.IsTrue(KeyCombination.TryParse("Control+S", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(KeyModifiers.Ctrl, Keys.S), combination);
    }

    [TestMethod]
    public void TryParse_CaseInsensitive()
    {
        Assert.IsTrue(KeyCombination.TryParse("ctrl+shift+z", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.Z), combination);
    }

    [TestMethod]
    public void TryParse_WhitespaceAroundSeparators_IsTolerated()
    {
        Assert.IsTrue(KeyCombination.TryParse(" Ctrl + Shift + S ", out KeyCombination combination));
        Assert.AreEqual(new KeyCombination(KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.S), combination);
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
            new(KeyModifiers.None, Keys.S),
            new(KeyModifiers.Ctrl, Keys.S),
            new(KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.Z),
            new(KeyModifiers.Alt, Keys.F4),
            new(KeyModifiers.Ctrl | KeyModifiers.Shift | KeyModifiers.Alt, Keys.Delete),
            new(KeyModifiers.None, Keys.Space),
            new(KeyModifiers.Ctrl, Keys.D7),
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
        Assert.AreEqual(new KeyCombination(KeyModifiers.Ctrl, Keys.S), KeyCombination.Parse("Ctrl+S"));
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
