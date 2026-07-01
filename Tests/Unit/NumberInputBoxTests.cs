using System.Collections.Generic;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class NumberInputBoxTests
{
    private sealed class FakeMouse : IMouse
    {
        public bool ButtonDown(MouseButton button) => false;
        public bool ButtonPressed(MouseButton button) => false;
        public bool ButtonReleased(MouseButton button) => false;
        public bool AnyButtonDown(IEnumerable<MouseButton> buttons) => false;
        public bool AnyButtonDown(params MouseButton[] buttons) => false;
        public bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => false;
        public bool AnyButtonPressed(params MouseButton[] buttons) => false;
        public bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => false;
        public bool AnyButtonReleased(params MouseButton[] buttons) => false;
        public bool AllButtonsDown(IEnumerable<MouseButton> buttons) => false;
        public bool AllButtonsDown(params MouseButton[] buttons) => false;
        public bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => false;
        public bool AllButtonsPressed(params MouseButton[] buttons) => false;
        public bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => false;
        public bool AllButtonsReleased(params MouseButton[] buttons) => false;
        public bool LeftDown => false;
        public bool MiddleDown => false;
        public bool RightDown => false;
        public bool LeftUp => true;
        public bool MiddleUp => true;
        public bool RightUp => true;
        public bool LeftPressed => false;
        public bool MiddlePressed => false;
        public bool RightPressed => false;
        public bool LeftReleased => false;
        public bool MiddleReleased => false;
        public bool RightReleased => false;
        public int ClientX => 0;
        public int ClientY => 0;
        public float WheelDelta => 0f;
        public int XDelta => 0;
        public int YDelta => 0;
    }

    private sealed class FakeKeyboard : IKeyboard
    {
        private readonly HashSet<Keys> _pressed = [];
        private readonly HashSet<Keys> _down = [];

        public void Press(params Keys[] keys)
        {
            foreach (Keys k in keys)
            {
                _pressed.Add(k);
                _down.Add(k);
            }
        }

        public void Hold(params Keys[] keys)
        {
            foreach (Keys k in keys)
            {
                _down.Add(k);
            }
        }

        public void Release()
        {
            _pressed.Clear();
            _down.Clear();
        }

        public bool KeyPressed(Keys key) => _pressed.Contains(key);
        public bool KeyDown(Keys key) => _down.Contains(key);
        public bool KeyReleased(Keys key) => false;

        public bool AnyKeyDown(IEnumerable<Keys> keys) => false;
        public bool AnyKeyDown(params Keys[] keys) => false;
        public bool AnyKeyPressed(IEnumerable<Keys> keys) => false;
        public bool AnyKeyPressed(params Keys[] keys) => false;
        public bool AnyKeyReleased(IEnumerable<Keys> keys) => false;
        public bool AnyKeyReleased(params Keys[] keys) => false;
        public bool AllKeysDown(IEnumerable<Keys> keys) => false;
        public bool AllKeysDown(params Keys[] keys) => false;
        public bool AllKeysPressed(IEnumerable<Keys> keys) => false;
        public bool AllKeysPressed(params Keys[] keys) => false;
        public bool AllKeysReleased(IEnumerable<Keys> keys) => false;
        public bool AllKeysReleased(params Keys[] keys) => false;
    }

    private sealed class FakeClipboard : IClipboard
    {
        public string Text { get; set; } = string.Empty;
        public string GetText() => Text;
        public void SetText(string text) => Text = text;
    }

    private FakeKeyboard _kb = null!;
    private FakeClipboard _clipboard = null!;

    [TestInitialize]
    public void Setup()
    {
        _kb = new FakeKeyboard();
        _clipboard = new FakeClipboard();
        EngineConfiguration.KeyboardService = _kb;
        EngineConfiguration.MouseService = new FakeMouse();
        EngineConfiguration.ClipboardService = _clipboard;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    private static NumberInputBox Make(bool allowDecimals = true, bool allowNegative = false,
                                       string initialText = "")
    {
        NumberInputBox box = new(0, 0, 200, 30, allowDecimals, allowNegative);
        if (initialText.Length > 0)
        {
            box.Text = initialText;
        }

        return box;
    }

    private void Update(NumberInputBox box) => box.Update(0.016);

    private void GoToEnd(NumberInputBox box)
    {
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
    }

    // -------------------------------------------------------------------------
    // IsCharAllowed — digits

    [TestMethod]
    public void TypeDigit_IsAccepted()
    {
        NumberInputBox box = Make();
        box.Focus();
        _kb.Press(Keys.D5);

        Update(box);

        Assert.AreEqual("5", box.Text);
    }

    [TestMethod]
    public void TypeLetter_IsRejected()
    {
        NumberInputBox box = Make();
        box.Focus();
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void TypeAllDigits_Accepted()
    {
        NumberInputBox box = Make();
        box.Focus();

        for (int i = 0; i <= 9; i++)
        {
            _kb.Press((Keys)((int)Keys.D0 + i));
            Update(box);
            _kb.Release();
        }

        Assert.AreEqual("0123456789", box.Text);
    }

    // -------------------------------------------------------------------------
    // IsCharAllowed — decimal point

    [TestMethod]
    public void TypePeriod_WhenAllowDecimals_IsAccepted()
    {
        NumberInputBox box = Make(allowDecimals: true);
        box.Focus();
        _kb.Press(Keys.Period);

        Update(box);

        Assert.AreEqual(".", box.Text);
    }

    [TestMethod]
    public void TypePeriod_WhenDecimalsNotAllowed_IsRejected()
    {
        NumberInputBox box = Make(allowDecimals: false);
        box.Focus();
        _kb.Press(Keys.Period);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void TypePeriod_WhenPeriodAlreadyPresent_IsRejected()
    {
        NumberInputBox box = Make(allowDecimals: true, initialText: "1.");
        GoToEnd(box);
        _kb.Press(Keys.Period);

        Update(box);

        Assert.AreEqual("1.", box.Text);
    }

    [TestMethod]
    public void TypePeriod_WhenNoPeriodPresent_IsAccepted()
    {
        NumberInputBox box = Make(allowDecimals: true, initialText: "1");
        GoToEnd(box);
        _kb.Press(Keys.Period);

        Update(box);

        Assert.AreEqual("1.", box.Text);
    }

    // -------------------------------------------------------------------------
    // IsCharAllowed — minus sign

    [TestMethod]
    public void TypeMinus_WhenAllowNegative_AtStart_IsAccepted()
    {
        NumberInputBox box = Make(allowNegative: true);
        box.Focus();
        _kb.Press(Keys.Minus);

        Update(box);

        Assert.AreEqual("-", box.Text);
    }

    [TestMethod]
    public void TypeMinus_WhenNotAllowNegative_IsRejected()
    {
        NumberInputBox box = Make(allowNegative: false);
        box.Focus();
        _kb.Press(Keys.Minus);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void TypeMinus_WhenCursorNotAtZero_IsRejected()
    {
        NumberInputBox box = Make(allowNegative: true, initialText: "5");
        GoToEnd(box);
        _kb.Press(Keys.Minus);

        Update(box);

        Assert.AreEqual("5", box.Text);
    }

    [TestMethod]
    public void TypeMinus_WhenAlreadyHasMinus_IsRejected()
    {
        NumberInputBox box = Make(allowNegative: true, initialText: "-5");
        // cursor back to 0
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Minus);

        Update(box);

        Assert.AreEqual("-5", box.Text);
    }

    // -------------------------------------------------------------------------
    // Value property

    [TestMethod]
    public void Value_WhenEmpty_ReturnsNull()
    {
        NumberInputBox box = Make();

        Assert.IsNull(box.Value);
    }

    [TestMethod]
    public void Value_WhenTextIsValidInt_ReturnsFloat()
    {
        NumberInputBox box = Make(initialText: "42");

        Assert.AreEqual(42f, box.Value);
    }

    [TestMethod]
    public void Value_WhenTextIsValidDecimal_ReturnsFloat()
    {
        NumberInputBox box = Make(allowDecimals: true, initialText: "3.14");

        Assert.AreEqual(3.14f, box.Value!.Value, delta: 0.001f);
    }

    [TestMethod]
    public void Value_WhenTextIsNegative_ReturnsNegativeFloat()
    {
        NumberInputBox box = Make(allowNegative: true, initialText: "-7");

        Assert.AreEqual(-7f, box.Value);
    }

    [TestMethod]
    public void Value_WhenTextIsJustMinus_ReturnsNull()
    {
        NumberInputBox box = Make(allowNegative: true, initialText: "-");

        Assert.IsNull(box.Value);
    }

    [TestMethod]
    public void Value_WhenTextIsJustPeriod_ReturnsNull()
    {
        NumberInputBox box = Make(allowDecimals: true, initialText: ".");

        Assert.IsNull(box.Value);
    }

    // -------------------------------------------------------------------------
    // MinValue / MaxValue (not enforced during typing, just settable)

    [TestMethod]
    public void MinValue_CanBeSetAndRead()
    {
        NumberInputBox box = Make();

        box.MinValue = -10f;

        Assert.AreEqual(-10f, box.MinValue);
    }

    [TestMethod]
    public void MaxValue_CanBeSetAndRead()
    {
        NumberInputBox box = Make();

        box.MaxValue = 100f;

        Assert.AreEqual(100f, box.MaxValue);
    }

    [TestMethod]
    public void Typing_DoesNotEnforceMaxValue_DuringEdit()
    {
        NumberInputBox box = Make(initialText: "99");
        box.MaxValue = 10f;
        GoToEnd(box);
        _kb.Press(Keys.D9);

        Update(box);

        // Max is not clamped during typing
        Assert.AreEqual("999", box.Text);
    }

    // -------------------------------------------------------------------------
    // Paste — filtered through IsCharAllowed just like keystrokes

    [TestMethod]
    public void Paste_NonNumericText_IsRejected()
    {
        NumberInputBox box = Make();
        box.Focus();
        _clipboard.Text = "abc";
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.V);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void Paste_MixedDigitsAndLetters_KeepsOnlyDigits()
    {
        NumberInputBox box = Make();
        box.Focus();
        _clipboard.Text = "1a2b3";
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.V);

        Update(box);

        Assert.AreEqual("123", box.Text);
    }

    [TestMethod]
    public void Paste_SecondDecimalPoint_IsRejected()
    {
        NumberInputBox box = Make(allowDecimals: true, initialText: "1.5");
        GoToEnd(box);
        _clipboard.Text = ".2";
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.V);

        Update(box);

        Assert.AreEqual("1.52", box.Text);
    }

    // -------------------------------------------------------------------------
    // Text setter validation — must mirror IsCharAllowed for keystrokes

    [TestMethod]
    public void TextSetter_LettersOnly_StripsThemAndReturnsEmpty()
    {
        NumberInputBox box = Make();

        box.Text = "abc";

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void TextSetter_MixedDigitsAndLetters_RetainsOnlyDigits()
    {
        NumberInputBox box = Make();

        box.Text = "1a2b3";

        Assert.AreEqual("123", box.Text);
    }

    [TestMethod]
    public void TextSetter_MultipleDecimalPoints_KeepsOnlyTheFirst()
    {
        NumberInputBox box = Make(allowDecimals: true);

        box.Text = "1.2.3";

        Assert.AreEqual("1.23", box.Text);
    }

    [TestMethod]
    public void TextSetter_DecimalPoint_WhenDecimalsDisallowed_IsStripped()
    {
        NumberInputBox box = Make(allowDecimals: false);

        box.Text = "1.5";

        Assert.AreEqual("15", box.Text);
    }

    [TestMethod]
    public void TextSetter_LeadingMinus_WhenAllowed_IsKept()
    {
        NumberInputBox box = Make(allowNegative: true);

        box.Text = "-5";

        Assert.AreEqual("-5", box.Text);
    }

    [TestMethod]
    public void TextSetter_LeadingMinus_WhenDisallowed_IsStripped()
    {
        NumberInputBox box = Make(allowNegative: false);

        box.Text = "-5";

        Assert.AreEqual("5", box.Text);
    }

    [TestMethod]
    public void TextSetter_MisplacedMinusInMiddle_IsStripped()
    {
        NumberInputBox box = Make(allowNegative: true);

        box.Text = "1-5";

        Assert.AreEqual("15", box.Text);
    }

    [TestMethod]
    public void TextSetter_AfterInvalidInput_ValueIsConsistentWithRemainingText()
    {
        NumberInputBox box = Make(allowDecimals: true);

        box.Text = "1.2.3";

        // After filtering: "1.23" → Value should be 1.23, not null
        Assert.IsNotNull(box.Value);
        Assert.AreEqual(1.23f, box.Value!.Value, delta: 0.001f);
    }
}
