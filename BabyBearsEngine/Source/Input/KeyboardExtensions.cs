namespace BabyBearsEngine.Input;

/// <summary>
/// <see cref="IKeyboard"/> helpers for testing whole <see cref="KeyCombination"/>s. Modifier
/// resolution treats the left and right variants of each modifier as equivalent — Ctrl in a
/// combination matches either <see cref="Keys.LeftControl"/> or <see cref="Keys.RightControl"/>
/// being held, and the same for Shift and Alt.
/// </summary>
public static class KeyboardExtensions
{
    /// <summary>
    /// Returns true when every modifier in <paramref name="combination"/>'s
    /// <see cref="KeyCombination.Modifiers"/> is currently held AND its
    /// <see cref="KeyCombination.Key"/> is currently held.
    /// </summary>
    public static bool CombinationDown(this IKeyboard keyboard, KeyCombination combination)
    {
        return ModifiersHeld(keyboard, combination.Modifiers) && keyboard.KeyDown(combination.Key);
    }

    /// <summary>
    /// Returns true on the single frame <paramref name="combination"/>'s
    /// <see cref="KeyCombination.Key"/> transitions from up to down, provided every modifier in
    /// <see cref="KeyCombination.Modifiers"/> is currently held. The modifiers do not need to
    /// have been pressed on the same frame — only held while the key edge-presses, which is the
    /// natural "Ctrl+S means tap S while holding Ctrl" semantics.
    /// </summary>
    public static bool CombinationPressed(this IKeyboard keyboard, KeyCombination combination)
    {
        return ModifiersHeld(keyboard, combination.Modifiers) && keyboard.KeyPressed(combination.Key);
    }

    private static bool ModifiersHeld(IKeyboard keyboard, KeyModifiers modifiers)
    {
        if ((modifiers & KeyModifiers.Ctrl) != 0
            && !keyboard.KeyDown(Keys.LeftControl)
            && !keyboard.KeyDown(Keys.RightControl))
        {
            return false;
        }

        if ((modifiers & KeyModifiers.Shift) != 0
            && !keyboard.KeyDown(Keys.LeftShift)
            && !keyboard.KeyDown(Keys.RightShift))
        {
            return false;
        }

        if ((modifiers & KeyModifiers.Alt) != 0
            && !keyboard.KeyDown(Keys.LeftAlt)
            && !keyboard.KeyDown(Keys.RightAlt))
        {
            return false;
        }

        return true;
    }
}
