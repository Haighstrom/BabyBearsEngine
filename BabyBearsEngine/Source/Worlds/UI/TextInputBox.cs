using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A single-line text editor with keyboard-driven cursor, selection, and character input.
/// Gain focus by clicking or by calling <see cref="Focus"/>; lose it with <see cref="Blur"/>
/// or by pressing Escape.
/// </summary>
/// <remarks>
/// <para>Character input is polled from the keyboard service each frame (no OS char event).
/// The key map assumes a standard US QWERTY layout.</para>
/// <para>Supported keys: printable characters (letters, digits, punctuation); Backspace; Delete;
/// Left / Right / Home / End (with optional Shift for selection); Ctrl+A (select all);
/// Ctrl+C / Ctrl+X / Ctrl+V (copy / cut / paste via the system clipboard);
/// Enter (fires <see cref="Submitted"/>); Escape (blurs).</para>
/// <para>A left press places the caret and, while held, dragging the mouse extends the
/// selection to follow it — the anchor stays fixed at the press point. Dragging past the
/// visible left edge scrolls to reveal earlier text; dragging past the right edge is handled
/// the same way cursor movement already scrolls into view.</para>
/// </remarks>
public class TextInputBox : Entity
{
    private const float ContentPadding = 4f;
    private const double CursorBlinkPeriod = 1.0;
    private const float CursorWidth = 2f;

    // Typematic repeat: holding a repeatable key (navigation, Backspace/Delete, typed
    // characters) waits RepeatInitialDelay before the first repeat, then re-fires every
    // RepeatInterval — the standard "press once, pause, then repeat at a steady rate" OS
    // text-field behaviour. Values are fixed rather than reading the OS's configured repeat
    // delay/rate (see #294 for why: that needs an engine-level char/text-input event stream).
    private const double RepeatInitialDelay = 0.5;
    private const double RepeatInterval = 0.04;

    private static readonly Dictionary<Keys, (char Normal, char Shifted)> s_charMap = BuildCharMap();

    private readonly IGraphic? _backgroundGraphic;
    private readonly ColourGraphic? _selectionGraphic;
    private readonly ITextGraphic? _textGraphic;
    private readonly ColourGraphic? _cursorGraphic;

    private string _text = "";
    private int _cursorIndex = 0;
    private int _anchorIndex = 0;
    private bool _isDragSelecting = false;
    private int _scrollOffset = 0;
    private double _blinkTimer = 0.0;
    private bool _hasFocus = false;
    private int _maxLength = 0;
    private bool _readOnly = false;
    private Keys? _repeatKey = null;
    private double _repeatTimer = 0.0;
    private bool _repeatArmed = false;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="initialText">Initial content. Defaults to empty.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public TextInputBox(float x, float y, float width, float height,
                        InputBoxTheme theme, string initialText = "", int layer = 0)
        : base(x, y, width, height, clickable: true, layer: layer)
    {
        _backgroundGraphic = theme.BackgroundFactory(new Rect(0, 0, width, height));
        Add(_backgroundGraphic);

        float contentY = ContentPadding;
        float contentH = height - 2f * ContentPadding;

        _selectionGraphic = new ColourGraphic(theme.SelectionColour, ContentPadding, contentY, 0f, contentH)
        {
            Visible = false
        };
        Add(_selectionGraphic);

        _textGraphic = new TextGraphic(
            theme.Text.Font, initialText, theme.Text.Colour,
            ContentPadding, 0f, width - 2f * ContentPadding, height)
        {
            HAlignment = HAlignment.Left,
            VAlignment = theme.Text.VAlignment,
            Multiline = false,
        };
        Add(_textGraphic);

        _cursorGraphic = new ColourGraphic(theme.CursorColour, ContentPadding, contentY, CursorWidth, contentH)
        {
            Visible = false
        };
        Add(_cursorGraphic);

        _text = initialText;
        _cursorIndex = 0;
        _anchorIndex = 0;
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="initialText">Initial content. Defaults to empty.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public TextInputBox(Rect rect, InputBoxTheme theme, string initialText = "", int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, initialText, layer)
    {
    }

    internal TextInputBox(float x, float y, float width, float height)
        : base(x, y, width, height, clickable: true)
    {
    }

    // Test-only constructor wired with a working text graphic (see StubTextGraphic) so mouse
    // hit-testing and drag-selection can be exercised without a GL-backed TextGraphic/theme.
    internal TextInputBox(float x, float y, float width, float height, ITextGraphic textGraphic)
        : base(x, y, width, height, clickable: true)
    {
        _textGraphic = textGraphic;
        Add(_textGraphic);
    }

    /// <summary>
    /// The current text content. Setting this truncates to <see cref="MaxLength"/> (when
    /// non-zero), moves the cursor to the end of the new text, and fires <see cref="TextChanged"/>.
    /// </summary>
    public virtual string Text
    {
        get => _text;
        set
        {
            // Apply MaxLength to programmatic assignments the same way TypeChar enforces it during
            // keystrokes, so setting Text can never bypass the limit.
            string truncated = _maxLength > 0 && value.Length > _maxLength ? value[.._maxLength] : value;

            if (_text == truncated)
            {
                return;
            }

            string old = _text;
            _text = truncated;
            // Cursor at end of new text — the caller replaced the content, so the previous cursor
            // position has no meaningful relationship to the new content.
            _cursorIndex = _text.Length;
            _anchorIndex = _cursorIndex;
            _scrollOffset = 0;
            UpdateDisplay();
            TextChanged?.Invoke(this, new TextChangedEventArgs(old, _text));
        }
    }

    /// <summary>
    /// Maximum number of characters allowed. 0 means no limit.
    /// </summary>
    public int MaxLength
    {
        get => _maxLength;
        set => _maxLength = Math.Max(0, value);
    }

    /// <summary>When true, keyboard input is suppressed; cursor navigation still works.</summary>
    public bool ReadOnly
    {
        get => _readOnly;
        set => _readOnly = value;
    }

    /// <summary>True when this box holds keyboard focus.</summary>
    public bool HasFocus => _hasFocus;

    /// <summary>Current cursor position within <see cref="Text"/> (0..Text.Length).</summary>
    public int CursorIndex => _cursorIndex;

    /// <summary>
    /// Index of the first character currently rendered (earlier characters are scrolled off).
    /// Exposed internally so unit tests can verify scroll position directly without a working
    /// text graphic.
    /// </summary>
    internal int ScrollOffset => _scrollOffset;

    /// <summary>Start of the current selection (inclusive). Equal to <see cref="SelectionEnd"/> when there is no selection.</summary>
    public int SelectionStart => Math.Min(_cursorIndex, _anchorIndex);

    /// <summary>End of the current selection (exclusive). Equal to <see cref="SelectionStart"/> when there is no selection.</summary>
    public int SelectionEnd => Math.Max(_cursorIndex, _anchorIndex);

    /// <summary>True when at least one character is selected.</summary>
    public bool HasSelection => _cursorIndex != _anchorIndex;

    // Process-wide focus arbiter: at most one TextInputBox holds keyboard focus across the engine.
    // Without this, clicking box A and then box B would leave both consuming every keystroke,
    // since each owns an independent _hasFocus flag and polls the global keyboard.
    private static TextInputBox? s_focusedBox;

    /// <summary>Gives this box keyboard focus, placing the cursor at the current position. Blurs any other currently-focused <see cref="TextInputBox"/>.</summary>
    public void Focus()
    {
        if (_hasFocus)
        {
            return;
        }

        s_focusedBox?.Blur();

        s_focusedBox = this;
        _hasFocus = true;
        _blinkTimer = 0.0;
        UpdateDisplay();
        FocusGained?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes keyboard focus. Any selection is collapsed to the cursor position.</summary>
    public void Blur()
    {
        if (!_hasFocus)
        {
            return;
        }

        if (s_focusedBox == this)
        {
            s_focusedBox = null;
        }

        _hasFocus = false;
        _anchorIndex = _cursorIndex;
        _isDragSelecting = false;

        _cursorGraphic?.Visible = false;

        _selectionGraphic?.Visible = false;

        FocusLost?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised whenever <see cref="Text"/> changes — whether by keyboard input or by setting the property.</summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <summary>Raised when the user presses Enter while this box has focus.</summary>
    public event EventHandler? Submitted;

    /// <summary>Raised when this box gains keyboard focus.</summary>
    public event EventHandler? FocusGained;

    /// <summary>Raised when this box loses keyboard focus.</summary>
    public event EventHandler? FocusLost;

    /// <inheritdoc/>
    protected override void OnRemoved()
    {
        // Container.Remove only unlinks parent — it doesn't notify us. Without this, a focused
        // box whose owning panel is closed would keep _hasFocus = true and continue consuming
        // keystrokes the next time it was re-added.
        Blur();
        base.OnRemoved();
    }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        if (_isDragSelecting)
        {
            if (Mouse.LeftDown)
            {
                ExtendSelectionToMouse();
            }
            else
            {
                _isDragSelecting = false;
            }
        }

        if (!_hasFocus)
        {
            return;
        }

        _blinkTimer += elapsed;

        HandleKeyboardInput(elapsed);
        UpdateCursorBlink();
    }

    /// <inheritdoc/>
    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();
        Focus();
        PlaceCursorAtMouse();
        _isDragSelecting = _textGraphic is not null;
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        if (_backgroundGraphic is not null)
        {
            _backgroundGraphic.Width = Width;
            _backgroundGraphic.Height = Height;
        }

        if (_textGraphic is not null)
        {
            _textGraphic.Width = Width - 2f * ContentPadding;
            _textGraphic.Height = Height;
        }

        float contentH = Height - 2f * ContentPadding;

        _cursorGraphic?.Height = contentH;

        _selectionGraphic?.Height = contentH;

        UpdateDisplay();
    }

    /// <summary>
    /// Returns true when <paramref name="c"/> is permitted to be typed into this box.
    /// Override to restrict the accepted character set (e.g. in <see cref="NumberInputBox"/>).
    /// </summary>
    protected virtual bool IsCharAllowed(char c) => true;

    private void HandleKeyboardInput(double elapsed)
    {
        bool shift = Keyboard.KeyDown(Keys.LeftShift) || Keyboard.KeyDown(Keys.RightShift);

        if (Keyboard.KeyPressed(Keys.Escape))
        {
            Blur();
            return;
        }

        if (Keyboard.KeyPressed(Keys.Enter) || Keyboard.KeyPressed(Keys.KeyPadEnter))
        {
            Submitted?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (Keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.A)))
        {
            SelectAll();
            return;
        }

        if (Keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.C)))
        {
            Copy();
            return;
        }

        if (Keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.X)))
        {
            Cut();
            return;
        }

        if (Keyboard.CombinationPressed(new KeyCombination(KeyModifiers.Ctrl, Keys.V)))
        {
            Paste();
            return;
        }

        if (!_readOnly)
        {
            if (ConsumeKeyPress(Keys.Backspace, elapsed))
            {
                HandleBackspace();
                return;
            }

            if (ConsumeKeyPress(Keys.Delete, elapsed))
            {
                HandleDelete();
                return;
            }

            char? typed = GetTypedChar(shift, elapsed);
            if (typed.HasValue && IsCharAllowed(typed.Value))
            {
                TypeChar(typed.Value);
                return;
            }
        }

        if (ConsumeKeyPress(Keys.Left, elapsed))
        {
            MoveLeft(shift);
        }
        else if (ConsumeKeyPress(Keys.Right, elapsed))
        {
            MoveRight(shift);
        }
        else if (ConsumeKeyPress(Keys.Home, elapsed))
        {
            MoveHome(shift);
        }
        else if (ConsumeKeyPress(Keys.End, elapsed))
        {
            MoveEnd(shift);
        }
    }

    /// <summary>
    /// Returns true on the frame <paramref name="key"/> is first pressed, and again on every
    /// typematic repeat once it has been held past <see cref="RepeatInitialDelay"/> — at a
    /// steady <see cref="RepeatInterval"/> cadence thereafter. Only one key repeats at a time:
    /// pressing a different repeatable key takes over as the active repeat key, matching how a
    /// physical keyboard's repeat follows whichever key was pressed last.
    /// </summary>
    private bool ConsumeKeyPress(Keys key, double elapsed)
    {
        if (Keyboard.KeyPressed(key))
        {
            _repeatKey = key;
            _repeatTimer = 0.0;
            _repeatArmed = false;
            return true;
        }

        if (_repeatKey != key)
        {
            return false;
        }

        if (!Keyboard.KeyDown(key))
        {
            _repeatKey = null;
            return false;
        }

        _repeatTimer += elapsed;
        double threshold = _repeatArmed ? RepeatInterval : RepeatInitialDelay;

        if (_repeatTimer < threshold)
        {
            return false;
        }

        // Preserve the overshoot past the threshold rather than resetting to zero, so the
        // repeat cadence doesn't drift when frame times don't divide it evenly.
        _repeatTimer -= threshold;
        _repeatArmed = true;
        return true;
    }

    private void HandleBackspace()
    {
        if (HasSelection)
        {
            DeleteSelection();
        }
        else if (_cursorIndex > 0)
        {
            string old = _text;
            _text = _text.Remove(_cursorIndex - 1, 1);
            _cursorIndex--;
            _anchorIndex = _cursorIndex;
            RaiseTextChanged(old);
        }

        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void HandleDelete()
    {
        if (HasSelection)
        {
            DeleteSelection();
        }
        else if (_cursorIndex < _text.Length)
        {
            string old = _text;
            _text = _text.Remove(_cursorIndex, 1);
            _anchorIndex = _cursorIndex;
            RaiseTextChanged(old);
        }

        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void TypeChar(char c)
    {
        if (HasSelection)
        {
            DeleteSelection();
        }

        if (_maxLength > 0 && _text.Length >= _maxLength)
        {
            return;
        }

        string old = _text;
        _text = _text.Insert(_cursorIndex, c.ToString());
        _cursorIndex++;
        _anchorIndex = _cursorIndex;
        _blinkTimer = 0.0;
        RaiseTextChanged(old);
        UpdateDisplay();
    }

    private void DeleteSelection()
    {
        int start = SelectionStart;
        int end = SelectionEnd;
        string old = _text;
        _text = _text.Remove(start, end - start);
        _cursorIndex = start;
        _anchorIndex = start;

        // Deleting a selection can leave _scrollOffset referring to a position that no longer
        // makes sense for the shortened text (e.g. Ctrl+A then Delete/typed-over on a scrolled,
        // overflowing box). EnsureScrollOffset's own correction only fires when the cursor is
        // still less than _scrollOffset at the next UpdateDisplay call — but a caller that inserts
        // new text right after this (TypeChar, Paste) advances the cursor first, so that check
        // would compare against the post-insert position instead of catching it here.
        _scrollOffset = Math.Min(_scrollOffset, _cursorIndex);

        RaiseTextChanged(old);
    }

    private void SelectAll()
    {
        _anchorIndex = 0;
        _cursorIndex = _text.Length;
        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void Copy()
    {
        if (!HasSelection)
        {
            return;
        }

        Clipboard.SetText(_text[SelectionStart..SelectionEnd]);
    }

    private void Cut()
    {
        if (_readOnly || !HasSelection)
        {
            return;
        }

        Clipboard.SetText(_text[SelectionStart..SelectionEnd]);
        DeleteSelection();
        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void Paste()
    {
        if (_readOnly)
        {
            return;
        }

        string clipboardText = Clipboard.GetText();
        if (clipboardText.Length == 0)
        {
            return;
        }

        if (HasSelection)
        {
            DeleteSelection();
        }

        string old = _text;
        bool inserted = false;

        // Newlines are stripped unconditionally rather than left to IsCharAllowed, since this
        // is a single-line control by construction (TextGraphic.Multiline = false) and typing
        // can never produce one — paste is the only path a newline could enter through.
        foreach (char c in clipboardText)
        {
            if (c == '\n' || c == '\r')
            {
                continue;
            }

            if (_maxLength > 0 && _text.Length >= _maxLength)
            {
                break;
            }

            if (!IsCharAllowed(c))
            {
                continue;
            }

            _text = _text.Insert(_cursorIndex, c.ToString());
            _cursorIndex++;
            inserted = true;
        }

        _anchorIndex = _cursorIndex;
        _blinkTimer = 0.0;

        if (inserted)
        {
            RaiseTextChanged(old);
        }

        UpdateDisplay();
    }

    private void MoveLeft(bool shift)
    {
        if (!shift && HasSelection)
        {
            _cursorIndex = SelectionStart;
            _anchorIndex = _cursorIndex;
        }
        else if (_cursorIndex > 0)
        {
            _cursorIndex--;
            if (!shift)
            {
                _anchorIndex = _cursorIndex;
            }
        }

        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void MoveRight(bool shift)
    {
        if (!shift && HasSelection)
        {
            _cursorIndex = SelectionEnd;
            _anchorIndex = _cursorIndex;
        }
        else if (_cursorIndex < _text.Length)
        {
            _cursorIndex++;
            if (!shift)
            {
                _anchorIndex = _cursorIndex;
            }
        }

        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void PlaceCursorAtMouse()
    {
        if (_textGraphic is null)
        {
            return;
        }

        int index = HitTestCursorIndex(_text, _scrollOffset, LocalMouseX, measured => _textGraphic.MeasureString(measured).X);

        _cursorIndex = index;
        _anchorIndex = index;
        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    // While a mouse-drag selection is in progress, follows the cursor to the mouse position each
    // frame — unlike PlaceCursorAtMouse, only _cursorIndex moves; _anchorIndex stays fixed at the
    // press point, growing or shrinking the selection as the mouse moves. Runs regardless of
    // whether the mouse is still over the box, so dragging past the box's edge and releasing
    // there still extends the selection correctly.
    private void ExtendSelectionToMouse()
    {
        if (_textGraphic is null)
        {
            return;
        }

        float localX = LocalMouseX;

        // HitTestCursorIndex can only return indices >= _scrollOffset, so a drag past the left
        // edge needs to retreat the scroll offset itself before hit-testing — revealing one more
        // character every frame for as long as the drag is held there. The right edge needs no
        // equivalent handling: HitTestCursorIndex already searches all the way to text.Length
        // regardless of the visible width, and UpdateDisplay's EnsureScrollOffset call advances
        // _scrollOffset to catch up to whatever index that produces.
        if (localX < 0f && _scrollOffset > 0)
        {
            _scrollOffset--;
        }

        _cursorIndex = HitTestCursorIndex(_text, _scrollOffset, localX, measured => _textGraphic.MeasureString(measured).X);
        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    // PositionOnScreen and the visible text both live in window space, so the mouse's distance
    // from the text area's left edge gives the local X to hit-test against character widths.
    // Negative when the mouse is to the left of the text area.
    private float LocalMouseX => Mouse.ClientX - (PositionOnScreen.X + ContentPadding);

    /// <summary>
    /// Returns the cursor index whose boundary is closest to <paramref name="localX"/> — the X offset
    /// (in pixels) from the left edge of the visible text. Characters before <paramref name="scrollOffset"/>
    /// are scrolled off-screen and excluded. The cursor lands before a character when the click falls
    /// left of that character's horizontal midpoint, matching standard text-field hit-testing.
    /// </summary>
    internal static int HitTestCursorIndex(string text, int scrollOffset, float localX, Func<string, float> measureWidth)
    {
        float widthToPrevious = 0f;

        for (int index = scrollOffset; index < text.Length; index++)
        {
            float widthToNext = measureWidth(text.Substring(scrollOffset, index + 1 - scrollOffset));
            float characterMidpoint = (widthToPrevious + widthToNext) / 2f;

            if (localX < characterMidpoint)
            {
                return index;
            }

            widthToPrevious = widthToNext;
        }

        return text.Length;
    }

    /// <summary>
    /// Clamps a selection highlight's raw (unclamped) start/end pixel positions to the content
    /// area's bounds, returning the highlight's final X and width.
    /// </summary>
    /// <remarks>
    /// <see cref="EnsureScrollOffset"/> only ever keeps the cursor in view, not the other end of
    /// the selection (the anchor) — e.g. Shift+Left (or a mouse drag) run past the left edge of
    /// the visible window jumps <c>_scrollOffset</c> back to follow the cursor, leaving the
    /// anchor positioned deep into the now-scrolled-off text to the right. Without clamping here,
    /// that measures to a pixel position beyond the box's own width and the highlight pokes out
    /// past the right edge.
    /// </remarks>
    internal static (float X, float Width) ClampSelectionBounds(float rawStartX, float rawEndX, float contentLeft, float contentRight)
    {
        float startX = Math.Clamp(rawStartX, contentLeft, contentRight);
        float endX = Math.Clamp(rawEndX, contentLeft, contentRight);
        return (startX, Math.Max(0f, endX - startX));
    }

    private void MoveHome(bool shift)
    {
        _cursorIndex = 0;
        if (!shift)
        {
            _anchorIndex = 0;
        }

        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void MoveEnd(bool shift)
    {
        _cursorIndex = _text.Length;
        if (!shift)
        {
            _anchorIndex = _cursorIndex;
        }

        _blinkTimer = 0.0;
        UpdateDisplay();
    }

    private void RaiseTextChanged(string old)
    {
        TextChanged?.Invoke(this, new TextChangedEventArgs(old, _text));
    }

    private void UpdateDisplay()
    {
        if (_textGraphic is null)
        {
            return;
        }

        EnsureScrollOffset();

        _textGraphic.Text = _text;

        // EnsureScrollOffset only ever guarantees the cursor fits within the visible width — it
        // says nothing about how much text remains after the cursor. TextGraphic's NumCharsToDraw
        // defaults to unbounded, so without this it would treat the entire remainder of the
        // string (all the way to _text.Length) as "should be visible" and flag a spurious
        // truncation warning for whatever tail overflows the box — which is the expected, normal
        // state of a scrolled box, not lost content. Bounding it to however many characters
        // actually fit keeps the warning meaningful for genuine over-sized text.
        _textGraphic.NumCharsToDraw = ComputeVisibleCharCount();

        // Cursor X — distance from left edge of the text area to the cursor
        float cursorX = ContentPadding + _textGraphic.MeasureString(
            _text.Substring(_scrollOffset, Math.Max(0, _cursorIndex - _scrollOffset))).X;

        _cursorGraphic?.X = cursorX;

        // Selection graphic
        if (_selectionGraphic is not null)
        {
            if (HasSelection && _hasFocus)
            {
                int visStart = Math.Max(SelectionStart, _scrollOffset);
                int visEnd = SelectionEnd;

                float rawStartX = ContentPadding + _textGraphic.MeasureString(
                    _text.Substring(_scrollOffset, Math.Max(0, visStart - _scrollOffset))).X;
                float rawEndX = ContentPadding + _textGraphic.MeasureString(
                    _text.Substring(_scrollOffset, Math.Max(0, visEnd - _scrollOffset))).X;

                (float selStartX, float selWidth) = ClampSelectionBounds(rawStartX, rawEndX, ContentPadding, Width - ContentPadding);

                _selectionGraphic.X = selStartX;
                _selectionGraphic.Width = selWidth;
                _selectionGraphic.Visible = true;
            }
            else
            {
                _selectionGraphic.Visible = false;
            }
        }

        _textGraphic.FirstCharToDraw = _scrollOffset;
    }

    private void EnsureScrollOffset()
    {
        if (_textGraphic is null)
        {
            return;
        }

        if (_cursorIndex < _scrollOffset)
        {
            _scrollOffset = _cursorIndex;
            return;
        }

        float availableWidth = Width - 2f * ContentPadding;

        float cursorX = _textGraphic.MeasureString(_text.Substring(_scrollOffset, _cursorIndex - _scrollOffset)).X;

        while (cursorX > availableWidth && _scrollOffset < _cursorIndex)
        {
            _scrollOffset++;
            cursorX = _textGraphic.MeasureString(_text.Substring(_scrollOffset, _cursorIndex - _scrollOffset)).X;
        }
    }

    // How many characters starting at _scrollOffset actually fit within the box's content
    // width. Mirrors EnsureScrollOffset's incremental measuring, but counts forward from the
    // scroll offset rather than back from the cursor, since the cursor may sit well before the
    // end of a long string and isn't a reliable bound on how much trailing text still shows.
    private int ComputeVisibleCharCount()
    {
        if (_textGraphic is null)
        {
            return 0;
        }

        float availableWidth = Width - 2f * ContentPadding;
        int remaining = _text.Length - _scrollOffset;
        int count = 0;

        while (count < remaining)
        {
            float width = _textGraphic.MeasureString(_text.Substring(_scrollOffset, count + 1)).X;
            if (width > availableWidth)
            {
                break;
            }

            count++;
        }

        return count;
    }

    private void UpdateCursorBlink()
    {
        if (_cursorGraphic is null)
        {
            return;
        }

        _cursorGraphic.Visible = _blinkTimer % CursorBlinkPeriod < CursorBlinkPeriod / 2.0;
    }

    private char? GetTypedChar(bool shift, double elapsed)
    {
        foreach (var (key, chars) in s_charMap)
        {
            if (ConsumeKeyPress(key, elapsed))
            {
                return shift ? chars.Shifted : chars.Normal;
            }
        }

        return null;
    }

    private static Dictionary<Keys, (char Normal, char Shifted)> BuildCharMap()
    {
        var map = new Dictionary<Keys, (char, char)>();

        // Letters A-Z (enum values match ASCII upper-case)
        for (int k = (int)Keys.A; k <= (int)Keys.Z; k++)
        {
            char upper = (char)k;
            map[(Keys)k] = (char.ToLower(upper), upper);
        }

        // Row digits and their shift symbols (US layout)
        map[Keys.D0] = ('0', ')');
        map[Keys.D1] = ('1', '!');
        map[Keys.D2] = ('2', '@');
        map[Keys.D3] = ('3', '#');
        map[Keys.D4] = ('4', '$');
        map[Keys.D5] = ('5', '%');
        map[Keys.D6] = ('6', '^');
        map[Keys.D7] = ('7', '&');
        map[Keys.D8] = ('8', '*');
        map[Keys.D9] = ('9', '(');

        // Punctuation
        map[Keys.Space]        = (' ',  ' ');
        map[Keys.Apostrophe]   = ('\'', '"');
        map[Keys.Comma]        = (',',  '<');
        map[Keys.Minus]        = ('-',  '_');
        map[Keys.Period]       = ('.',  '>');
        map[Keys.Slash]        = ('/',  '?');
        map[Keys.Semicolon]    = (';',  ':');
        map[Keys.Equal]        = ('=',  '+');
        map[Keys.LeftBracket]  = ('[',  '{');
        map[Keys.Backslash]    = ('\\', '|');
        map[Keys.RightBracket] = (']',  '}');
        map[Keys.GraveAccent]  = ('`',  '~');

        // Keypad
        for (int k = (int)Keys.KeyPad0; k <= (int)Keys.KeyPad9; k++)
        {
            char digit = (char)('0' + (k - (int)Keys.KeyPad0));
            map[(Keys)k] = (digit, digit);
        }

        map[Keys.KeyPadDecimal]  = ('.', '.');
        map[Keys.KeyPadDivide]   = ('/', '/');
        map[Keys.KeyPadMultiply] = ('*', '*');
        map[Keys.KeyPadSubtract] = ('-', '-');
        map[Keys.KeyPadAdd]      = ('+', '+');

        return map;
    }
}
