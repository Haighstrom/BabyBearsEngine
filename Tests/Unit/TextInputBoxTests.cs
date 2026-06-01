using System.Collections.Generic;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TextInputBoxTests
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

    // Fake keyboard where individual keys can be held or pressed per-call
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

    private FakeKeyboard _kb = null!;

    [TestInitialize]
    public void Setup()
    {
        _kb = new FakeKeyboard();
        EngineConfiguration.KeyboardService = _kb;
        EngineConfiguration.MouseService = new FakeMouse();
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    private static TextInputBox Make(string initialText = "")
    {
        TextInputBox box = new(0, 0, 200, 30);
        box.Text = initialText;
        return box;
    }

    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) => entity.Parent = null;
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private void Update(TextInputBox box) => box.Update(0.016);

    // -------------------------------------------------------------------------
    // Initial state

    [TestMethod]
    public void Text_DefaultsToEmpty()
    {
        TextInputBox box = new(0, 0, 200, 30);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void HasFocus_DefaultsFalse()
    {
        TextInputBox box = Make();

        Assert.IsFalse(box.HasFocus);
    }

    [TestMethod]
    public void CursorIndex_DefaultsToZero()
    {
        TextInputBox box = Make();

        Assert.AreEqual(0, box.CursorIndex);
    }

    [TestMethod]
    public void HasSelection_DefaultsFalse()
    {
        TextInputBox box = Make();

        Assert.IsFalse(box.HasSelection);
    }

    // -------------------------------------------------------------------------
    // Text property

    [TestMethod]
    public void Text_Set_UpdatesValue()
    {
        TextInputBox box = Make();

        box.Text = "hello";

        Assert.AreEqual("hello", box.Text);
    }

    [TestMethod]
    public void Text_Set_FiresTextChanged()
    {
        TextInputBox box = Make("abc");
        string? captured = null;
        box.TextChanged += (_, e) => captured = e.NewText;

        box.Text = "xyz";

        Assert.AreEqual("xyz", captured);
    }

    [TestMethod]
    public void Text_Set_TextChangedEventArgs_CarriesOldAndNew()
    {
        TextInputBox box = Make("old");
        string? oldText = null;
        string? newText = null;
        box.TextChanged += (_, e) => { oldText = e.OldText; newText = e.NewText; };

        box.Text = "new";

        Assert.AreEqual("old", oldText);
        Assert.AreEqual("new", newText);
    }

    [TestMethod]
    public void Text_Set_SameValue_DoesNotFireTextChanged()
    {
        TextInputBox box = Make("same");
        int count = 0;
        box.TextChanged += (_, _) => count++;

        box.Text = "same";

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void Text_Set_ClampsCursorToNewLength()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();

        box.Text = "hi";

        Assert.AreEqual(2, box.CursorIndex);
    }

    // -------------------------------------------------------------------------
    // Focus / Blur

    [TestMethod]
    public void Focus_SetsHasFocus()
    {
        TextInputBox box = Make();

        box.Focus();

        Assert.IsTrue(box.HasFocus);
    }

    [TestMethod]
    public void Blur_ClearsHasFocus()
    {
        TextInputBox box = Make();
        box.Focus();

        box.Blur();

        Assert.IsFalse(box.HasFocus);
    }

    [TestMethod]
    public void FocusGained_FiresOnFocus()
    {
        TextInputBox box = Make();
        int count = 0;
        box.FocusGained += (_, _) => count++;

        box.Focus();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void FocusGained_DoesNotFireIfAlreadyFocused()
    {
        TextInputBox box = Make();
        box.Focus();
        int count = 0;
        box.FocusGained += (_, _) => count++;

        box.Focus();

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void FocusLost_FiresOnBlur()
    {
        TextInputBox box = Make();
        box.Focus();
        int count = 0;
        box.FocusLost += (_, _) => count++;

        box.Blur();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void FocusLost_DoesNotFireIfNotFocused()
    {
        TextInputBox box = Make();
        int count = 0;
        box.FocusLost += (_, _) => count++;

        box.Blur();

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void RemovedFromParent_WhileFocused_ClearsFocus()
    {
        TextInputBox box = Make();
        box.Parent = new FakeContainer();
        box.Focus();
        Assert.IsTrue(box.HasFocus);

        box.Parent = null;

        Assert.IsFalse(box.HasFocus);
    }

    [TestMethod]
    public void RemovedFromParent_WhileFocused_FiresFocusLost()
    {
        TextInputBox box = Make();
        box.Parent = new FakeContainer();
        box.Focus();
        int count = 0;
        box.FocusLost += (_, _) => count++;

        box.Parent = null;

        Assert.AreEqual(1, count);
    }

    // -------------------------------------------------------------------------
    // Keyboard ignored when not focused

    [TestMethod]
    public void Update_WhenNotFocused_IgnoresKeyPresses()
    {
        TextInputBox box = Make();
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    // -------------------------------------------------------------------------
    // Character input

    [TestMethod]
    public void Update_TypeA_AppendsLowerA()
    {
        TextInputBox box = Make();
        box.Focus();
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual("a", box.Text);
    }

    [TestMethod]
    public void Update_TypeAWithShift_AppendsUpperA()
    {
        TextInputBox box = Make();
        box.Focus();
        _kb.Hold(Keys.LeftShift);
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual("A", box.Text);
    }

    [TestMethod]
    public void Update_TypeCharacter_AdvancesCursorByOne()
    {
        TextInputBox box = Make();
        box.Focus();
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual(1, box.CursorIndex);
    }

    [TestMethod]
    public void Update_TypeCharacter_InsertsAtCursorPosition()
    {
        TextInputBox box = Make("bc");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual("abc", box.Text);
    }

    [TestMethod]
    public void Update_TypeCharacter_FiresTextChanged()
    {
        TextInputBox box = Make();
        box.Focus();
        string? captured = null;
        box.TextChanged += (_, e) => captured = e.NewText;
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual("a", captured);
    }

    [TestMethod]
    public void Update_TypeDigit_AppendsDigit()
    {
        TextInputBox box = Make();
        box.Focus();
        _kb.Press(Keys.D5);

        Update(box);

        Assert.AreEqual("5", box.Text);
    }

    [TestMethod]
    public void Update_TypeSpace_AppendsSpace()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Space);

        Update(box);

        Assert.AreEqual("hello ", box.Text);
    }

    // -------------------------------------------------------------------------
    // Backspace / Delete

    [TestMethod]
    public void Update_Backspace_DeletesCharBeforeCursor()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Backspace);

        Update(box);

        Assert.AreEqual("a", box.Text);
    }

    [TestMethod]
    public void Update_Backspace_AtStart_IsNoOp()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Backspace);

        Update(box);

        Assert.AreEqual("ab", box.Text);
    }

    [TestMethod]
    public void Update_Delete_DeletesCharAfterCursor()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Delete);

        Update(box);

        Assert.AreEqual("b", box.Text);
        Assert.AreEqual(0, box.CursorIndex);
    }

    [TestMethod]
    public void Update_Delete_AtEnd_IsNoOp()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Delete);

        Update(box);

        Assert.AreEqual("ab", box.Text);
    }

    // -------------------------------------------------------------------------
    // Enter / Escape

    [TestMethod]
    public void Update_Enter_FiresSubmitted()
    {
        TextInputBox box = Make();
        box.Focus();
        int count = 0;
        box.Submitted += (_, _) => count++;
        _kb.Press(Keys.Enter);

        Update(box);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Update_Escape_BlursBox()
    {
        TextInputBox box = Make();
        box.Focus();
        _kb.Press(Keys.Escape);

        Update(box);

        Assert.IsFalse(box.HasFocus);
    }

    [TestMethod]
    public void Update_Escape_FiresFocusLost()
    {
        TextInputBox box = Make();
        box.Focus();
        int count = 0;
        box.FocusLost += (_, _) => count++;
        _kb.Press(Keys.Escape);

        Update(box);

        Assert.AreEqual(1, count);
    }

    // -------------------------------------------------------------------------
    // Cursor navigation

    [TestMethod]
    public void Update_Left_MovesCursorBack()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Left);

        Update(box);

        Assert.AreEqual(1, box.CursorIndex);
    }

    [TestMethod]
    public void Update_Left_AtStart_IsNoOp()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Left);

        Update(box);

        Assert.AreEqual(0, box.CursorIndex);
    }

    [TestMethod]
    public void Update_Right_MovesCursorForward()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Right);

        Update(box);

        Assert.AreEqual(1, box.CursorIndex);
    }

    [TestMethod]
    public void Update_Right_AtEnd_IsNoOp()
    {
        TextInputBox box = Make("ab");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Right);

        Update(box);

        Assert.AreEqual(2, box.CursorIndex);
    }

    [TestMethod]
    public void Update_Home_MovesCursorToStart()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Home);

        Update(box);

        Assert.AreEqual(0, box.CursorIndex);
    }

    [TestMethod]
    public void Update_End_MovesCursorToEnd()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Press(Keys.End);

        Update(box);

        Assert.AreEqual(5, box.CursorIndex);
    }

    // -------------------------------------------------------------------------
    // Selection

    [TestMethod]
    public void Update_ShiftRight_ExtendSelection()
    {
        TextInputBox box = Make("abc");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        _kb.Hold(Keys.LeftShift);
        _kb.Press(Keys.Right);

        Update(box);

        Assert.IsTrue(box.HasSelection);
        Assert.AreEqual(0, box.SelectionStart);
        Assert.AreEqual(1, box.SelectionEnd);
    }

    [TestMethod]
    public void Update_ShiftLeft_ExtendSelectionLeft()
    {
        TextInputBox box = Make("abc");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Hold(Keys.LeftShift);
        _kb.Press(Keys.Left);

        Update(box);

        Assert.IsTrue(box.HasSelection);
        Assert.AreEqual(2, box.SelectionStart);
        Assert.AreEqual(3, box.SelectionEnd);
    }

    [TestMethod]
    public void Update_CtrlA_SelectsAllText()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.A);

        Update(box);

        Assert.IsTrue(box.HasSelection);
        Assert.AreEqual(0, box.SelectionStart);
        Assert.AreEqual(5, box.SelectionEnd);
    }

    [TestMethod]
    public void Update_Left_WithSelection_CollapsesCursorToSelectionStart()
    {
        TextInputBox box = Make("abc");
        box.Focus();
        // Select all
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.A);
        Update(box);
        _kb.Release();
        // Press left without shift
        _kb.Press(Keys.Left);

        Update(box);

        Assert.IsFalse(box.HasSelection);
        Assert.AreEqual(0, box.CursorIndex);
    }

    [TestMethod]
    public void Update_Right_WithSelection_CollapsesCursorToSelectionEnd()
    {
        TextInputBox box = Make("abc");
        box.Focus();
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.A);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Right);

        Update(box);

        Assert.IsFalse(box.HasSelection);
        Assert.AreEqual(3, box.CursorIndex);
    }

    [TestMethod]
    public void Update_TypeWithSelection_DeletesSelectionFirst()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.A);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.X);

        Update(box);

        Assert.AreEqual("x", box.Text);
        Assert.IsFalse(box.HasSelection);
    }

    [TestMethod]
    public void Update_BackspaceWithSelection_DeletesSelection()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.A);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Backspace);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void Update_DeleteWithSelection_DeletesSelection()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Hold(Keys.LeftControl);
        _kb.Press(Keys.A);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Delete);

        Update(box);

        Assert.AreEqual(string.Empty, box.Text);
    }

    [TestMethod]
    public void SelectionStart_LessThanOrEqualToSelectionEnd()
    {
        TextInputBox box = Make("abc");
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        // shift+Left to select backwards
        _kb.Hold(Keys.LeftShift);
        _kb.Press(Keys.Left);
        Update(box);
        _kb.Release();

        Assert.IsLessThan(box.SelectionEnd, box.SelectionStart);
    }

    // -------------------------------------------------------------------------
    // MaxLength

    [TestMethod]
    public void MaxLength_Zero_AllowsUnlimitedInput()
    {
        TextInputBox box = Make();
        box.MaxLength = 0;
        box.Focus();

        for (int i = 0; i < 100; i++)
        {
            _kb.Press(Keys.A);
            Update(box);
            _kb.Release();
        }

        Assert.AreEqual(100, box.Text.Length);
    }

    [TestMethod]
    public void MaxLength_EnforcedOnTyping()
    {
        TextInputBox box = Make("ab");
        box.MaxLength = 2;
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.C);

        Update(box);

        Assert.AreEqual("ab", box.Text);
    }

    [TestMethod]
    public void MaxLength_AllowsTypingUpToLimit()
    {
        TextInputBox box = Make("a");
        box.MaxLength = 2;
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.B);

        Update(box);

        Assert.AreEqual("ab", box.Text);
    }

    [TestMethod]
    public void MaxLength_EnforcedOnTextSetter()
    {
        TextInputBox box = Make();
        box.MaxLength = 5;

        box.Text = "hello world";

        Assert.AreEqual("hello", box.Text);
    }

    [TestMethod]
    public void MaxLength_Zero_TextSetterAcceptsAnyLength()
    {
        TextInputBox box = Make();
        box.MaxLength = 0;

        box.Text = new string('x', 1000);

        Assert.AreEqual(1000, box.Text.Length);
    }

    [TestMethod]
    public void Text_Set_MovesCursorToEndOfNewText()
    {
        TextInputBox box = Make();
        // Cursor starts at 0; without the fix it stays at 0 after setting Text.

        box.Text = "hello";

        Assert.AreEqual(5, box.CursorIndex);
    }

    // -------------------------------------------------------------------------
    // ReadOnly

    [TestMethod]
    public void ReadOnly_BlocksCharacterInput()
    {
        TextInputBox box = Make("hello");
        box.ReadOnly = true;
        box.Focus();
        _kb.Press(Keys.A);

        Update(box);

        Assert.AreEqual("hello", box.Text);
    }

    [TestMethod]
    public void ReadOnly_BlocksBackspace()
    {
        TextInputBox box = Make("hello");
        box.ReadOnly = true;
        box.Focus();
        _kb.Press(Keys.End);
        Update(box);
        _kb.Release();
        _kb.Press(Keys.Backspace);

        Update(box);

        Assert.AreEqual("hello", box.Text);
    }

    [TestMethod]
    public void ReadOnly_AllowsCursorNavigation()
    {
        TextInputBox box = Make("hello");
        box.Focus();
        _kb.Press(Keys.Home);
        Update(box);
        _kb.Release();
        box.ReadOnly = true;
        _kb.Press(Keys.Right);

        Update(box);

        Assert.AreEqual(1, box.CursorIndex);
    }

    // -------------------------------------------------------------------------
    // OnLeftClicked → Focus

    [TestMethod]
    public void OnLeftClicked_GainsFocus()
    {
        TextInputBox box = Make();
        // Internal constructor exposes OnLeftClicked through the protected method
        TestInputBox test = new();

        test.FireClick();

        Assert.IsTrue(test.HasFocus);
    }

    private sealed class TestInputBox : TextInputBox
    {
        internal TestInputBox() : base(0, 0, 200, 30) { }
        internal void FireClick() => OnLeftClicked();
    }
}
