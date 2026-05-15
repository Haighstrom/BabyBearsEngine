using System.Collections.Generic;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="Button"/> that owns an ordered list of values and cycles through them on each
/// left-click. The button's label is updated to show the current value (using
/// <see cref="object.ToString"/> by default, or a caller-supplied formatter).
/// </summary>
public class CyclingValueButton<T> : Button
{
    private readonly IReadOnlyList<T> _values;
    private readonly Func<T, string> _formatter;
    private int _currentIndex = 0;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the button.</param>
    /// <param name="values">The values to cycle through. Must contain at least one element.</param>
    /// <param name="formatter">Optional formatter producing the label for each value. Defaults to <see cref="object.ToString"/>.</param>
    /// <param name="initialIndex">Index of the initially-selected value. Defaults to 0.</param>
    public CyclingValueButton(
        float x,
        float y,
        float width,
        float height,
        ButtonTheme theme,
        IReadOnlyList<T> values,
        Func<T, string>? formatter = null,
        int initialIndex = 0)
        : base(x, y, width, height, theme)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
        {
            throw new ArgumentException("Must contain at least one value.", nameof(values));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(initialIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(initialIndex, values.Count);

        _values = values;
        _formatter = formatter ?? (v => v?.ToString() ?? string.Empty);
        _currentIndex = initialIndex;

        Text = _formatter(CurrentValue);
    }

    /// <summary>The currently selected value.</summary>
    public T CurrentValue => _values[_currentIndex];

    /// <summary>The index of <see cref="CurrentValue"/> in the values list.</summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>Raised after a click cycles to the next value, with the old and new values.</summary>
    public event EventHandler<CyclingValueChangedEventArgs<T>>? ValueChanged;

    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();

        T oldValue = CurrentValue;
        _currentIndex = (_currentIndex + 1) % _values.Count;
        T newValue = CurrentValue;

        Text = _formatter(newValue);
        ValueChanged?.Invoke(this, new CyclingValueChangedEventArgs<T>(oldValue, newValue));
    }
}
