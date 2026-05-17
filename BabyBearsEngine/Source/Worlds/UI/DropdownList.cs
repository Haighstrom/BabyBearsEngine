using System.Collections.Generic;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A dropdown selector. In its closed state it shows a single header button displaying the
/// current selection. Clicking the header opens a vertical list of option buttons as direct
/// children of this entity. If the list would overflow the bottom of the viewport it opens
/// upward instead. Clicking an option closes the list and fires <see cref="SelectionChanged"/>.
/// </summary>
/// <typeparam name="T">The type of the selectable values.</typeparam>
public class DropdownList<T> : Entity
{
    private int _currentIndex = 0;
    private readonly Func<T, string> _formatter;
    private readonly Func<float> _getViewportHeight;
    private bool _isOpen = false;
    private readonly IReadOnlyList<T> _items;
    private readonly Func<int, string, Entity> _optionFactory;
    private Entity? _optionList;
    private readonly Action<string> _setHeaderText;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels — also used as the height of each option row.</param>
    /// <param name="items">The values to choose from. Must contain at least one element.</param>
    /// <param name="theme">Visual styling for the header and option buttons.</param>
    /// <param name="formatter">Optional label formatter. Defaults to <see cref="object.ToString"/>.</param>
    /// <param name="initialIndex">Index of the initially selected item. Defaults to 0.</param>
    public DropdownList(
        float x,
        float y,
        float width,
        float height,
        IReadOnlyList<T> items,
        DropdownListTheme theme,
        Func<T, string>? formatter = null,
        int initialIndex = 0)
        : base(x, y, width, height)
    {
        ValidateArgs(items, initialIndex);

        _items = items;
        _formatter = formatter ?? (v => v?.ToString() ?? string.Empty);
        _currentIndex = initialIndex;
        _getViewportHeight = static () => OpenGLHelper.GetViewport().Height;

        Button header = new(0f, 0f, width, height, theme.Header, _formatter(_items[initialIndex]));
        header.LeftClicked += (_, _) => Toggle();
        Add(header);
        _setHeaderText = t => header.Text = t;
        _optionFactory = (i, label) => new Button(0f, i * height, width, height, theme.Option, label);
    }

    /// <summary>
    /// Testable constructor that accepts a pre-built header entity and an option factory,
    /// bypassing the need for an OpenGL context. Internal only — use the public constructor
    /// in production code.
    /// </summary>
    internal DropdownList(
        float x,
        float y,
        float width,
        float height,
        IReadOnlyList<T> items,
        Entity header,
        Action<string> setHeaderText,
        Func<int, string, Entity> optionFactory,
        Func<T, string>? formatter = null,
        int initialIndex = 0,
        float viewportHeight = float.MaxValue)
        : base(x, y, width, height)
    {
        ValidateArgs(items, initialIndex);

        _items = items;
        _formatter = formatter ?? (v => v?.ToString() ?? string.Empty);
        _currentIndex = initialIndex;
        _getViewportHeight = () => viewportHeight;

        header.LeftClicked += (_, _) => Toggle();
        Add(header);
        _setHeaderText = setHeaderText;
        _optionFactory = optionFactory;
    }

    /// <summary>The index of the currently selected item.</summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>The currently selected value.</summary>
    public T CurrentValue => _items[_currentIndex];

    /// <summary>Whether the option list is currently open.</summary>
    public bool IsOpen => _isOpen;

    /// <summary>Raised when the selection changes. Not raised when the same item is selected again.</summary>
    public event EventHandler<SelectionChangedEventArgs<T>>? SelectionChanged;

    /// <summary>Closes the option list. No-op if already closed.</summary>
    public void Close()
    {
        if (!_isOpen || _optionList is null)
        {
            return;
        }

        _isOpen = false;
        Remove(_optionList);
        _optionList = null;
    }

    /// <summary>Opens the option list. No-op if already open.</summary>
    public void Open()
    {
        if (_isOpen)
        {
            return;
        }

        _isOpen = true;

        float listHeight = _items.Count * Height;
        var (_, headerBottomWindowY) = GetWindowCoordinates(0f, Height);
        float localListY = CalculateListLocalY(Height, listHeight, headerBottomWindowY, _getViewportHeight());

        _optionList = new Entity(0f, localListY, Width, listHeight);

        for (int i = 0; i < _items.Count; i++)
        {
            int capturedIndex = i;
            Entity option = _optionFactory(i, _formatter(_items[i]));
            option.LeftClicked += (_, _) => SelectItem(capturedIndex);
            _optionList.Add(option);
        }

        Add(_optionList);
    }

    /// <summary>Selects the item at <paramref name="index"/>, closes the list, and raises <see cref="SelectionChanged"/>.</summary>
    internal void SelectItem(int index)
    {
        T oldValue = CurrentValue;
        _currentIndex = index;
        T newValue = CurrentValue;

        _setHeaderText(_formatter(newValue));
        Close();

        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs<T>(oldValue, newValue));
        }
    }

    /// <summary>
    /// Returns the local Y position at which the option list should be placed. Opens below
    /// the header (<paramref name="headerHeight"/>) when the list fits in the viewport;
    /// opens above (<c>-<paramref name="listHeight"/></c>) otherwise.
    /// </summary>
    internal static float CalculateListLocalY(
        float headerHeight, float listHeight, float headerBottomWindowY, float viewportHeight)
    {
        return (headerBottomWindowY + listHeight <= viewportHeight) ? headerHeight : -listHeight;
    }

    private static void ValidateArgs(IReadOnlyList<T> items, int initialIndex)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("Must contain at least one item.", nameof(items));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(initialIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(initialIndex, items.Count);
    }

    private void Toggle()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }
}
