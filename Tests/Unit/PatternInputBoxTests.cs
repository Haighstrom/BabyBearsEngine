using System.Collections.Generic;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PatternInputBoxTests
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

    private static PatternInputBox Make(string pattern = "###-###", bool forceUpperCase = false, Func<char, bool>? isCharAllowed = null)
    {
        PatternInputBox box = new(0, 0, 200, 30, pattern, isCharAllowed, forceUpperCase);
        box.Focus();
        return box;
    }

    private void Update(PatternInputBox box) => box.Update(0.016);

    private void Press(PatternInputBox box, Keys key)
    {
        _kb.Release();
        _kb.Press(key);
        Update(box);
        _kb.Release();
    }

    private void TypeKeys(PatternInputBox box, params Keys[] keys)
    {
        foreach (Keys key in keys)
        {
            Press(box, key);
        }
    }

    private void Paste(PatternInputBox box, string text)
    {
        _clipboard.Text = text;
        _kb.Release();
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.V);
        Update(box);
        _kb.Release();
    }

    [TestMethod]
    public void Typing_InsertsSeparatorBeforeFirstCharOfNextGroup()
    {
        PatternInputBox box = Make();

        TypeKeys(box, Keys.K, Keys.D7, Keys.Q);
        Assert.AreEqual("k7q", box.Text);

        TypeKeys(box, Keys.D2);
        Assert.AreEqual("k7q-2", box.Text);
    }

    [TestMethod]
    public void Typing_FullPattern_StopsAtPatternLength()
    {
        PatternInputBox box = Make();

        TypeKeys(box, Keys.K, Keys.D7, Keys.Q, Keys.D2, Keys.X, Keys.D9, Keys.A);

        Assert.AreEqual("k7q-2x9", box.Text);
        Assert.AreEqual("k7q2x9", box.RawText);
    }

    [TestMethod]
    public void Typing_ForceUpperCase_UpperCasesText()
    {
        PatternInputBox box = Make(forceUpperCase: true);

        TypeKeys(box, Keys.K, Keys.D7, Keys.Q, Keys.D2);

        Assert.AreEqual("K7Q-2", box.Text);
    }

    [TestMethod]
    public void Typing_SeparatorKey_IsRejected()
    {
        PatternInputBox box = Make();

        TypeKeys(box, Keys.K, Keys.Minus);

        Assert.AreEqual("k", box.Text);
    }

    [TestMethod]
    public void Typing_CharRejectedByFilter_IsIgnored()
    {
        PatternInputBox box = Make(isCharAllowed: char.IsDigit);

        TypeKeys(box, Keys.K, Keys.D7);

        Assert.AreEqual("7", box.Text);
    }

    [TestMethod]
    public void Backspace_AfterSeparatorGroupStart_RemovesSeparatorToo()
    {
        PatternInputBox box = Make();
        TypeKeys(box, Keys.K, Keys.D7, Keys.Q, Keys.D2);

        Press(box, Keys.Backspace);

        Assert.AreEqual("k7q", box.Text);
    }

    [TestMethod]
    public void Backspace_RemovesPreviousCharacterAndKeepsTyping()
    {
        PatternInputBox box = Make();
        TypeKeys(box, Keys.K, Keys.D7, Keys.Q, Keys.D2, Keys.X);

        Press(box, Keys.Backspace);
        Press(box, Keys.D9);

        Assert.AreEqual("k7q-29", box.Text);
    }

    [TestMethod]
    public void Paste_WithoutSeparator_InsertsSeparator()
    {
        PatternInputBox box = Make();

        Paste(box, "K7Q2X9");

        Assert.AreEqual("K7Q-2X9", box.Text);
    }

    [TestMethod]
    public void Paste_WithSeparator_GivesSameResult()
    {
        PatternInputBox box = Make();

        Paste(box, "K7Q-2X9");

        Assert.AreEqual("K7Q-2X9", box.Text);
    }

    [TestMethod]
    public void Paste_TooLong_IsTruncatedToPattern()
    {
        PatternInputBox box = Make();

        Paste(box, "K7Q2X9ABC");

        Assert.AreEqual("K7Q-2X9", box.Text);
    }

    [TestMethod]
    public void Paste_ForceUpperCase_UpperCasesText()
    {
        PatternInputBox box = Make(forceUpperCase: true);

        Paste(box, "k7q2x9");

        Assert.AreEqual("K7Q-2X9", box.Text);
    }

    [TestMethod]
    public void TextSetter_AcceptsTextWithOrWithoutSeparators()
    {
        PatternInputBox box = Make();

        box.Text = "K7Q2X9";
        Assert.AreEqual("K7Q-2X9", box.Text);

        box.Text = "K7Q-2X9";
        Assert.AreEqual("K7Q-2X9", box.Text);
    }

    [TestMethod]
    public void TextSetter_DropsDisallowedCharacters()
    {
        PatternInputBox box = Make(isCharAllowed: char.IsLetterOrDigit);

        box.Text = "K7 Q!2X9";

        Assert.AreEqual("K7Q-2X9", box.Text);
    }

    [TestMethod]
    public void TextChanged_FiresOnceWithFormattedText()
    {
        PatternInputBox box = Make();
        TypeKeys(box, Keys.K, Keys.D7, Keys.Q);
        List<string> seen = [];
        box.TextChanged += (s, e) => seen.Add(box.Text);

        TypeKeys(box, Keys.D2);

        Assert.HasCount(1, seen);
        Assert.AreEqual("k7q-2", seen[0]);
    }

    [TestMethod]
    public void InsertingMidText_KeepsCaretAfterInsertedCharacter()
    {
        PatternInputBox box = Make();
        box.Text = "K7Q2X";
        Press(box, Keys.Home);
        Press(box, Keys.Right);

        Press(box, Keys.D5);

        Assert.AreEqual("K57-Q2X", box.Text);
        Assert.AreEqual(2, box.CursorIndex);
    }

    [TestMethod]
    public void Pattern_MixedGroupSizesAndSeparators_AreFollowed()
    {
        PatternInputBox box = Make("##/####");

        Paste(box, "123456");

        Assert.AreEqual("12/3456", box.Text);
    }

    [TestMethod]
    public void Constructor_PatternWithoutSlot_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new PatternInputBox(0, 0, 200, 30, "---"));
    }
}
