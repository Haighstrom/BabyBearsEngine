using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MouseFacadeTests
{
    private sealed class FakeMouse : IMouse
    {
        public List<(string Method, object? Arg)> Calls { get; } = [];
        public bool ReturnValue { get; set; }

        public bool LeftDown { get; set; }
        public bool MiddleDown { get; set; }
        public bool RightDown { get; set; }
        public bool LeftUp { get; set; }
        public bool MiddleUp { get; set; }
        public bool RightUp { get; set; }
        public bool LeftPressed { get; set; }
        public bool MiddlePressed { get; set; }
        public bool RightPressed { get; set; }
        public bool LeftReleased { get; set; }
        public bool MiddleReleased { get; set; }
        public bool RightReleased { get; set; }
        public int ClientX { get; set; }
        public int ClientY { get; set; }
        public float WheelDelta { get; set; }
        public int XDelta { get; set; }
        public int YDelta { get; set; }

        private bool Record(string method, object arg)
        {
            Calls.Add((method, arg));
            return ReturnValue;
        }

        public bool ButtonDown(MouseButton button) => Record(nameof(ButtonDown), button);
        public bool ButtonPressed(MouseButton button) => Record(nameof(ButtonPressed), button);
        public bool ButtonReleased(MouseButton button) => Record(nameof(ButtonReleased), button);
        public bool AnyButtonDown(IEnumerable<MouseButton> buttons) => Record(nameof(AnyButtonDown) + "/Enum", buttons);
        public bool AnyButtonDown(params MouseButton[] buttons) => Record(nameof(AnyButtonDown) + "/Params", buttons);
        public bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => Record(nameof(AnyButtonPressed) + "/Enum", buttons);
        public bool AnyButtonPressed(params MouseButton[] buttons) => Record(nameof(AnyButtonPressed) + "/Params", buttons);
        public bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => Record(nameof(AnyButtonReleased) + "/Enum", buttons);
        public bool AnyButtonReleased(params MouseButton[] buttons) => Record(nameof(AnyButtonReleased) + "/Params", buttons);
        public bool AllButtonsDown(IEnumerable<MouseButton> buttons) => Record(nameof(AllButtonsDown) + "/Enum", buttons);
        public bool AllButtonsDown(params MouseButton[] buttons) => Record(nameof(AllButtonsDown) + "/Params", buttons);
        public bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => Record(nameof(AllButtonsPressed) + "/Enum", buttons);
        public bool AllButtonsPressed(params MouseButton[] buttons) => Record(nameof(AllButtonsPressed) + "/Params", buttons);
        public bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => Record(nameof(AllButtonsReleased) + "/Enum", buttons);
        public bool AllButtonsReleased(params MouseButton[] buttons) => Record(nameof(AllButtonsReleased) + "/Params", buttons);
    }

    private FakeMouse _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeMouse();
        EngineConfiguration.MouseService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void Member_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Mouse.LeftDown);
    }

    // Per-button methods

    [TestMethod]
    public void ButtonDown_PassesButton_AndReturnsServiceValue()
    {
        _fake.ReturnValue = true;
        bool result = Mouse.ButtonDown(MouseButton.Left);
        Assert.IsTrue(result);
        Assert.AreEqual(("ButtonDown", (object?)MouseButton.Left), _fake.Calls.Single());
    }

    [TestMethod]
    public void ButtonPressed_PassesButton_AndReturnsServiceValue()
    {
        _fake.ReturnValue = true;
        bool result = Mouse.ButtonPressed(MouseButton.Right);
        Assert.IsTrue(result);
        Assert.AreEqual(("ButtonPressed", (object?)MouseButton.Right), _fake.Calls.Single());
    }

    [TestMethod]
    public void ButtonReleased_PassesButton_AndReturnsServiceValue()
    {
        _fake.ReturnValue = false;
        bool result = Mouse.ButtonReleased(MouseButton.Middle);
        Assert.IsFalse(result);
        Assert.AreEqual(("ButtonReleased", (object?)MouseButton.Middle), _fake.Calls.Single());
    }

    // Multi-button overloads

    [TestMethod]
    public void AnyButtonDown_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<MouseButton> buttons = [MouseButton.Left];
        Mouse.AnyButtonDown(buttons);
        Assert.AreEqual("AnyButtonDown/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyButtonDown_Params_RoutesToParamsOverload()
    {
        Mouse.AnyButtonDown(MouseButton.Left);
        Assert.AreEqual("AnyButtonDown/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyButtonPressed_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<MouseButton> buttons = [MouseButton.Left];
        Mouse.AnyButtonPressed(buttons);
        Assert.AreEqual("AnyButtonPressed/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyButtonPressed_Params_RoutesToParamsOverload()
    {
        Mouse.AnyButtonPressed(MouseButton.Left);
        Assert.AreEqual("AnyButtonPressed/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyButtonReleased_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<MouseButton> buttons = [MouseButton.Left];
        Mouse.AnyButtonReleased(buttons);
        Assert.AreEqual("AnyButtonReleased/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AnyButtonReleased_Params_RoutesToParamsOverload()
    {
        Mouse.AnyButtonReleased(MouseButton.Left);
        Assert.AreEqual("AnyButtonReleased/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllButtonsDown_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<MouseButton> buttons = [MouseButton.Left];
        Mouse.AllButtonsDown(buttons);
        Assert.AreEqual("AllButtonsDown/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllButtonsDown_Params_RoutesToParamsOverload()
    {
        Mouse.AllButtonsDown(MouseButton.Left);
        Assert.AreEqual("AllButtonsDown/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllButtonsPressed_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<MouseButton> buttons = [MouseButton.Left];
        Mouse.AllButtonsPressed(buttons);
        Assert.AreEqual("AllButtonsPressed/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllButtonsPressed_Params_RoutesToParamsOverload()
    {
        Mouse.AllButtonsPressed(MouseButton.Left);
        Assert.AreEqual("AllButtonsPressed/Params", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllButtonsReleased_Enumerable_RoutesToEnumerableOverload()
    {
        IEnumerable<MouseButton> buttons = [MouseButton.Left];
        Mouse.AllButtonsReleased(buttons);
        Assert.AreEqual("AllButtonsReleased/Enum", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void AllButtonsReleased_Params_RoutesToParamsOverload()
    {
        Mouse.AllButtonsReleased(MouseButton.Left);
        Assert.AreEqual("AllButtonsReleased/Params", _fake.Calls.Single().Method);
    }

    // Button state properties — paired (down/up/pressed/released for L/M/R)

    [TestMethod]
    public void ButtonStateProperties_RouteToServiceValues()
    {
        _fake.LeftDown = true;
        _fake.MiddleDown = true;
        _fake.RightDown = true;
        _fake.LeftUp = true;
        _fake.MiddleUp = true;
        _fake.RightUp = true;
        _fake.LeftPressed = true;
        _fake.MiddlePressed = true;
        _fake.RightPressed = true;
        _fake.LeftReleased = true;
        _fake.MiddleReleased = true;
        _fake.RightReleased = true;

        Assert.IsTrue(Mouse.LeftDown);
        Assert.IsTrue(Mouse.MiddleDown);
        Assert.IsTrue(Mouse.RightDown);
        Assert.IsTrue(Mouse.LeftUp);
        Assert.IsTrue(Mouse.MiddleUp);
        Assert.IsTrue(Mouse.RightUp);
        Assert.IsTrue(Mouse.LeftPressed);
        Assert.IsTrue(Mouse.MiddlePressed);
        Assert.IsTrue(Mouse.RightPressed);
        Assert.IsTrue(Mouse.LeftReleased);
        Assert.IsTrue(Mouse.MiddleReleased);
        Assert.IsTrue(Mouse.RightReleased);
    }

    [TestMethod]
    public void ButtonStateProperties_DefaultFalse_FromServiceValues()
    {
        Assert.IsFalse(Mouse.LeftDown);
        Assert.IsFalse(Mouse.MiddleDown);
        Assert.IsFalse(Mouse.RightDown);
        Assert.IsFalse(Mouse.LeftPressed);
        Assert.IsFalse(Mouse.MiddleReleased);
    }

    // Position / motion / wheel

    [TestMethod]
    public void ClientXY_RouteToServiceValues()
    {
        _fake.ClientX = 123;
        _fake.ClientY = 456;
        Assert.AreEqual(123, Mouse.ClientX);
        Assert.AreEqual(456, Mouse.ClientY);
    }

    [TestMethod]
    public void DeltaProperties_RouteToServiceValues()
    {
        _fake.XDelta = 7;
        _fake.YDelta = -3;
        _fake.WheelDelta = 1.5f;
        Assert.AreEqual(7, Mouse.XDelta);
        Assert.AreEqual(-3, Mouse.YDelta);
        Assert.AreEqual(1.5f, Mouse.WheelDelta);
    }

    // Service substitution after install

    [TestMethod]
    public void ReplacingService_RoutesToNewInstance()
    {
        var second = new FakeMouse { LeftDown = true };
        EngineConfiguration.MouseService = second;
        Assert.IsTrue(Mouse.LeftDown);
        Assert.IsFalse(_fake.LeftDown);
    }
}
