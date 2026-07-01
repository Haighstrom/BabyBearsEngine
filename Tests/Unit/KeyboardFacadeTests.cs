using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class KeyboardFacadeTests
{
    private sealed class FakeKeyboard : IKeyboard
    {
        public List<(string Method, object? Arg)> Calls { get; } = [];
        public bool ReturnValue { get; set; }

        private bool Record(string method, object arg)
        {
            Calls.Add((method, arg));
            return ReturnValue;
        }

        public bool KeyDown(Keys key) => Record(nameof(KeyDown), key);
        public bool KeyPressed(Keys key) => Record(nameof(KeyPressed), key);
        public bool KeyReleased(Keys key) => Record(nameof(KeyReleased), key);
        public bool AnyKeyDown(IEnumerable<Keys> keys) => Record(nameof(AnyKeyDown) + "/Enum", keys);
        public bool AnyKeyDown(params Keys[] keys) => Record(nameof(AnyKeyDown) + "/Params", keys);
        public bool AnyKeyPressed(IEnumerable<Keys> keys) => Record(nameof(AnyKeyPressed) + "/Enum", keys);
        public bool AnyKeyPressed(params Keys[] keys) => Record(nameof(AnyKeyPressed) + "/Params", keys);
        public bool AnyKeyReleased(IEnumerable<Keys> keys) => Record(nameof(AnyKeyReleased) + "/Enum", keys);
        public bool AnyKeyReleased(params Keys[] keys) => Record(nameof(AnyKeyReleased) + "/Params", keys);
        public bool AllKeysDown(IEnumerable<Keys> keys) => Record(nameof(AllKeysDown) + "/Enum", keys);
        public bool AllKeysDown(params Keys[] keys) => Record(nameof(AllKeysDown) + "/Params", keys);
        public bool AllKeysPressed(IEnumerable<Keys> keys) => Record(nameof(AllKeysPressed) + "/Enum", keys);
        public bool AllKeysPressed(params Keys[] keys) => Record(nameof(AllKeysPressed) + "/Params", keys);
        public bool AllKeysReleased(IEnumerable<Keys> keys) => Record(nameof(AllKeysReleased) + "/Enum", keys);
        public bool AllKeysReleased(params Keys[] keys) => Record(nameof(AllKeysReleased) + "/Params", keys);
    }

    private FakeKeyboard _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeKeyboard();
        EngineConfiguration.KeyboardService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    // Service-not-installed contract

    [TestMethod]
    public void Member_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Keyboard.KeyDown(Keys.A));
    }

    // Single-key methods

    [TestMethod]
    public void KeyDown_PassesKey_AndReturnsServiceValue()
    {
        _fake.ReturnValue = true;
        bool result = Keyboard.KeyDown(Keys.Space);
        Assert.IsTrue(result);
        Assert.AreEqual(("KeyDown", (object?)Keys.Space), _fake.Calls.Single());
    }

    [TestMethod]
    public void KeyPressed_PassesKey_AndReturnsServiceValue()
    {
        _fake.ReturnValue = true;
        bool result = Keyboard.KeyPressed(Keys.Enter);
        Assert.IsTrue(result);
        Assert.AreEqual(("KeyPressed", (object?)Keys.Enter), _fake.Calls.Single());
    }

    [TestMethod]
    public void KeyReleased_PassesKey_AndReturnsServiceValue()
    {
        _fake.ReturnValue = false;
        bool result = Keyboard.KeyReleased(Keys.Escape);
        Assert.IsFalse(result);
        Assert.AreEqual(("KeyReleased", (object?)Keys.Escape), _fake.Calls.Single());
    }

    // Multi-key Any/All — both overloads

    [TestMethod]
    public void AnyKeyDown_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<Keys> keys = [Keys.A, Keys.B];
        Keyboard.AnyKeyDown(keys);
        Assert.AreEqual("AnyKeyDown/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyKeyDown_Params_RoutesToParamsOverload()
    {
        Keyboard.AnyKeyDown(Keys.A, Keys.B);
        Assert.AreEqual("AnyKeyDown/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyKeyPressed_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<Keys> keys = [Keys.A];
        Keyboard.AnyKeyPressed(keys);
        Assert.AreEqual("AnyKeyPressed/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyKeyPressed_Params_RoutesToParamsOverload()
    {
        Keyboard.AnyKeyPressed(Keys.A);
        Assert.AreEqual("AnyKeyPressed/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyKeyReleased_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<Keys> keys = [Keys.A];
        Keyboard.AnyKeyReleased(keys);
        Assert.AreEqual("AnyKeyReleased/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyKeyReleased_Params_RoutesToParamsOverload()
    {
        Keyboard.AnyKeyReleased(Keys.A);
        Assert.AreEqual("AnyKeyReleased/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllKeysDown_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<Keys> keys = [Keys.A];
        Keyboard.AllKeysDown(keys);
        Assert.AreEqual("AllKeysDown/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllKeysDown_Params_RoutesToParamsOverload()
    {
        Keyboard.AllKeysDown(Keys.A);
        Assert.AreEqual("AllKeysDown/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllKeysPressed_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<Keys> keys = [Keys.A];
        Keyboard.AllKeysPressed(keys);
        Assert.AreEqual("AllKeysPressed/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllKeysPressed_Params_RoutesToParamsOverload()
    {
        Keyboard.AllKeysPressed(Keys.A);
        Assert.AreEqual("AllKeysPressed/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllKeysReleased_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<Keys> keys = [Keys.A];
        Keyboard.AllKeysReleased(keys);
        Assert.AreEqual("AllKeysReleased/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllKeysReleased_Params_RoutesToParamsOverload()
    {
        Keyboard.AllKeysReleased(Keys.A);
        Assert.AreEqual("AllKeysReleased/Params", _fake.Calls.Single().Method);
    }

    // Key combinations

    [TestMethod]
    public void CombinationDown_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationPressed_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.S)));
    }

    [TestMethod]
    public void CombinationDown_RoutesThroughInstalledService()
    {
        // The extension method calls KeyDown on the service for both the modifier(s) and the key.
        _fake.ReturnValue = true;
        bool result = Keyboard.CombinationDown(new KeyCombination(KeyModifiers.Ctrl, Keys.S));
        Assert.IsTrue(result);
        Assert.IsNotEmpty(_fake.Calls);
        Assert.AreEqual("KeyDown", _fake.Calls[0].Method);
    }

    [TestMethod]
    public void CombinationPressed_RoutesThroughInstalledService()
    {
        // Modifier resolution calls KeyDown, then the key check calls KeyPressed.
        _fake.ReturnValue = true;
        bool result = Keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.S));
        Assert.IsTrue(result);
        Assert.Contains(("KeyPressed", (object?)Keys.S), _fake.Calls);
    }

    // Service substitution after install

    [TestMethod]
    public void ReplacingService_RoutesToNewInstance()
    {
        var second = new FakeKeyboard { ReturnValue = true };
        EngineConfiguration.KeyboardService = second;

        Keyboard.KeyDown(Keys.X);

        Assert.IsEmpty(_fake.Calls);
        Assert.HasCount(1, second.Calls);
    }
}
