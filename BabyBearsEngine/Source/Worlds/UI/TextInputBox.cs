using System.Collections.Generic;
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
/// Enter (fires <see cref="Submitted"/>); Escape (blurs).</para>
/// </remarks>
public class TextInputBox : Entity
{
    private const float ContentPadding = 4f;
    private const double CursorBlinkPeriod = 1.0;
    private const float CursorWidth = 2f;

    private static readonly Dictionary<Keys, (char Normal, char Shifted)> s_charMap = BuildCharMap();

    private readonly IGraphic? _backgroundGraphic;
    private readonly ColourGraphic? _selectionGraphic;
    private readonly TextGraphic? _textGraphic;
    private readonly ColourGraphic? _cursorGraphic;

    private string _text = "";
    private int _cursorIndex = 0;
    private int _anchorIndex = 0;
    private int _scrollOffset = 0;
    private double _blinkTimer = 0.0;
    private bool _hasFocus = false;
    private int _maxLength = 0;
    private bool _readOnly = false;

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

    /// <summary>
    /// The current text content. Setting this resets the cursor to the start and fires
    /// <see cref="TextChanged"/>.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
            {
                return;
            }

            string old = _text;
            _text = value;
            _cursorIndex = Math.Clamp(_cursorIndex, 0, _text.Length);
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

    /// <summary>Start of the current selection (inclusive). Equal to <see cref="SelectionEnd"/> when there is no selection.</summary>
    public int SelectionStart => Math.Min(_cursorIndex, _anchorIndex);

    /// <summary>End of the current selection (exclusive). Equal to <see cref="SelectionStart"/> when there is no selection.</summary>
    public int SelectionEnd => Math.Max(_cursorIndex, _anchorIndex);

    /// <summary>True when at least one character is selected.</summary>
    public bool HasSelection => _cursorIndex != _anchorIndex;

    // internal so tests can verify without exposing as public API
    internal int AnchorIndex => _anchorIndex;

    /// <summary>Gives this box keyboard focus, placing the cursor at the current position.</summary>
    public void Focus()
    {
        if (_hasFocus)
        {
            return;
        }

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

        _hasFocus = false;
        _anchorIndex = _cursorIndex;

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
    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        if (!_hasFocus)
        {
            return;
        }

        _blinkTimer += elapsed;

        HandleKeyboardInput();
        UpdateCursorBlink();
    }

    /// <inheritdoc/>
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();
        Focus();
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

    private void HandleKeyboardInput()
    {
        bool shift = Keyboard.KeyDown(Keys.LeftShift) || Keyboard.KeyDown(Keys.RightShift);
        bool ctrl = Keyboard.KeyDown(Keys.LeftControl) || Keyboard.KeyDown(Keys.RightControl);

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

        if (ctrl)
        {
            if (Keyboard.KeyPressed(Keys.A))
            {
                SelectAll();
                return;
            }
        }

        if (!_readOnly)
        {
            if (Keyboard.KeyPressed(Keys.Backspace))
            {
                HandleBackspace();
                return;
            }

            if (Keyboard.KeyPressed(Keys.Delete))
            {
                HandleDelete();
                return;
            }

            char? typed = GetTypedChar(shift);
            if (typed.HasValue && IsCharAllowed(typed.Value))
            {
                TypeChar(typed.Value);
                return;
            }
        }

        if (Keyboard.KeyPressed(Keys.Left))
        {
            MoveLeft(shift);
        }
        else if (Keyboard.KeyPressed(Keys.Right))
        {
            MoveRight(shift);
        }
        else if (Keyboard.KeyPressed(Keys.Home))
        {
            MoveHome(shift);
        }
        else if (Keyboard.KeyPressed(Keys.End))
        {
            MoveEnd(shift);
        }
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
        RaiseTextChanged(old);
    }

    private void SelectAll()
    {
        _anchorIndex = 0;
        _cursorIndex = _text.Length;
        _blinkTimer = 0.0;
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

        // Cursor X — distance from left edge of the text area to the cursor
        string beforeCursor = _text.Substring(_scrollOffset,
            Math.Max(0, _cursorIndex - _scrollOffset));
        float cursorX = ContentPadding + _textGraphic.MeasureString(beforeCursor).X;

        _cursorGraphic?.X = cursorX;

        // Selection graphic
        if (_selectionGraphic is not null)
        {
            if (HasSelection && _hasFocus)
            {
                int visStart = Math.Max(SelectionStart, _scrollOffset);
                int visEnd = SelectionEnd;

                float selStartX = ContentPadding + _textGraphic.MeasureString(
                    _text.Substring(_scrollOffset, Math.Max(0, visStart - _scrollOffset))).X;
                float selEndX = ContentPadding + _textGraphic.MeasureString(
                    _text.Substring(_scrollOffset, Math.Max(0, visEnd - _scrollOffset))).X;

                _selectionGraphic.X = selStartX;
                _selectionGraphic.Width = Math.Max(0f, selEndX - selStartX);
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

        string visibleBeforeCursor = _text[_scrollOffset.._cursorIndex];
        float cursorX = _textGraphic.MeasureString(visibleBeforeCursor).X;

        while (cursorX > availableWidth && _scrollOffset < _cursorIndex)
        {
            _scrollOffset++;
            visibleBeforeCursor = _text[_scrollOffset.._cursorIndex];
            cursorX = _textGraphic.MeasureString(visibleBeforeCursor).X;
        }
    }

    private void UpdateCursorBlink()
    {
        if (_cursorGraphic is null)
        {
            return;
        }

        _cursorGraphic.Visible = _blinkTimer % CursorBlinkPeriod < CursorBlinkPeriod / 2.0;
    }

    private static char? GetTypedChar(bool shift)
    {
        foreach (var (key, chars) in s_charMap)
        {
            if (Keyboard.KeyPressed(key))
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
