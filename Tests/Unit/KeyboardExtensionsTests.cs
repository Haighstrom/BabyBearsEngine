using System;
using System.Collections.Generic;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class KeyboardExtensionsTests
{
    private sealed class StatefulFakeKeyboard : IKeyboard
    {
        public HashSet<Keys> Held { get; } = [];
        public HashSet<Keys> EdgePressed { get; } = [];
        public HashSet<Keys> EdgeReleased { get; } = [];

        public bool KeyDown(Keys key) => Held.Contains(key);
        public bool KeyPressed(Keys key) => EdgePressed.Contains(key);
        public bool KeyReleased(Keys key) => EdgeReleased.Contains(key);

        public bool AnyKeyDown(IEnumerable<Keys> keys) => throw new NotImplementedException();
        public bool AnyKeyDown(params Keys[] keys) => throw new NotImplementedException();
        public bool AnyKeyPressed(IEnumerable<Keys> keys) => throw new NotImplementedException();
        public bool AnyKeyPressed(params Keys[] keys) => throw new NotImplementedException();
        public bool AnyKeyReleased(IEnumerable<Keys> keys) => throw new NotImplementedException();
        public bool AnyKeyReleased(params Keys[] keys) => throw new NotImplementedException();
        public bool AllKeysDown(IEnumerable<Keys> keys) => throw new NotImplementedException();
        public bool AllKeysDown(params Keys[] keys) => throw new NotImplementedException();
        public bool AllKeysPressed(IEnumerable<Keys> keys) => throw new NotImplementedException();
        public bool AllKeysPressed(params Keys[] keys) => throw new NotImplementedException();
        public bool AllKeysReleased(IEnumerable<Keys> keys) => throw new NotImplementedException();
        public bool AllKeysReleased(params Keys[] keys) => throw new NotImplementedException();
    }

    // CombinationDown — happy path

    [TestMethod]
    public void CombinationDown_NoModifiers_OnlyKeyMatters()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.S);
        Assert.IsTrue(keyboard.CombinationDown(new KeyCombination(KeyModifiers.None, Keys.S)));
    }

    [TestMethod]
    public void CombinationDown_KeyNotHeld_ReturnsFalse()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftControl);
        Assert.IsFalse(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationDown_AllRequiredHeld_ReturnsTrue()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftControl);
        keyboard.Held.Add(Keys.S);
        Assert.IsTrue(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    // Left/Right modifier symmetry

    [TestMethod]
    public void CombinationDown_RightControl_SatisfiesCtrlRequirement()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.RightControl);
        keyboard.Held.Add(Keys.S);
        Assert.IsTrue(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationDown_RightShift_SatisfiesShiftRequirement()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.RightShift);
        keyboard.Held.Add(Keys.A);
        Assert.IsTrue(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Shift, Keys.A)));
    }

    [TestMethod]
    public void CombinationDown_RightAlt_SatisfiesAltRequirement()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.RightAlt);
        keyboard.Held.Add(Keys.F4);
        Assert.IsTrue(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Alt, Keys.F4)));
    }

    // Missing modifier

    [TestMethod]
    public void CombinationDown_ModifierNotHeld_ReturnsFalse()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.S);
        Assert.IsFalse(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationDown_WrongModifierHeld_ReturnsFalse()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftShift);
        keyboard.Held.Add(Keys.S);
        Assert.IsFalse(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationDown_MultipleModifiers_AllRequired()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftControl);
        keyboard.Held.Add(Keys.Z);
        Assert.IsFalse(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.Z)));

        keyboard.Held.Add(Keys.RightShift);
        Assert.IsTrue(keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.Z)));
    }

    // CombinationPressed — edge semantics

    [TestMethod]
    public void CombinationPressed_KeyHeldButNotEdgePressed_ReturnsFalse()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftControl);
        keyboard.Held.Add(Keys.S);
        Assert.IsFalse(keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationPressed_KeyEdgePressedWhileModifierHeld_ReturnsTrue()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftControl);
        keyboard.Held.Add(Keys.S);
        keyboard.EdgePressed.Add(Keys.S);
        Assert.IsTrue(keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationPressed_ModifierNotHeld_ReturnsFalseEvenIfKeyEdgePressed()
    {
        StatefulFakeKeyboard keyboard = new();
        keyboard.EdgePressed.Add(Keys.S);
        Assert.IsFalse(keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationPressed_ModifierDoesNotNeedToBeEdgePressed_OnlyHeld()
    {
        // The whole point: holding Ctrl from a previous frame and then tapping S should fire.
        StatefulFakeKeyboard keyboard = new();
        keyboard.Held.Add(Keys.LeftControl); // held — NOT in EdgePressed
        keyboard.Held.Add(Keys.S);
        keyboard.EdgePressed.Add(Keys.S);
        Assert.IsTrue(keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }
}
