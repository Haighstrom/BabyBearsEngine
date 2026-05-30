using BabyBearsEngine.Input;

namespace BabyBearsEngine;

/// <summary>
/// Static facade over the installed <see cref="IKeyboard"/> service. All members route through
/// <c>EngineConfiguration.KeyboardService</c>; tests substitute a fake there to exercise consumers
/// without a real input device. Throws <see cref="InvalidOperationException"/> if accessed before the engine is initialised.
/// </summary>
public static class Keyboard
{
    private static IKeyboard Implementation => EngineConfiguration.KeyboardService;

    /// <inheritdoc cref="IKeyboard.KeyDown(Keys)"/>
    public static bool KeyDown(Keys key) => Implementation.KeyDown(key);

    /// <inheritdoc cref="IKeyboard.KeyPressed(Keys)"/>
    public static bool KeyPressed(Keys key) => Implementation.KeyPressed(key);

    /// <inheritdoc cref="IKeyboard.KeyReleased(Keys)"/>
    public static bool KeyReleased(Keys key) => Implementation.KeyReleased(key);

    /// <inheritdoc cref="IKeyboard.AnyKeyDown(IEnumerable{Keys})"/>
    public static bool AnyKeyDown(IEnumerable<Keys> keys) => Implementation.AnyKeyDown(keys);

    /// <inheritdoc cref="IKeyboard.AnyKeyDown(Keys[])"/>
    public static bool AnyKeyDown(params Keys[] keys) => Implementation.AnyKeyDown(keys);

    /// <inheritdoc cref="IKeyboard.AnyKeyPressed(IEnumerable{Keys})"/>
    public static bool AnyKeyPressed(IEnumerable<Keys> keys) => Implementation.AnyKeyPressed(keys);

    /// <inheritdoc cref="IKeyboard.AnyKeyPressed(Keys[])"/>
    public static bool AnyKeyPressed(params Keys[] keys) => Implementation.AnyKeyPressed(keys);

    /// <inheritdoc cref="IKeyboard.AnyKeyReleased(IEnumerable{Keys})"/>
    public static bool AnyKeyReleased(IEnumerable<Keys> keys) => Implementation.AnyKeyReleased(keys);

    /// <inheritdoc cref="IKeyboard.AnyKeyReleased(Keys[])"/>
    public static bool AnyKeyReleased(params Keys[] keys) => Implementation.AnyKeyReleased(keys);

    /// <inheritdoc cref="IKeyboard.AllKeysDown(IEnumerable{Keys})"/>
    public static bool AllKeysDown(IEnumerable<Keys> keys) => Implementation.AllKeysDown(keys);

    /// <inheritdoc cref="IKeyboard.AllKeysDown(Keys[])"/>
    public static bool AllKeysDown(params Keys[] keys) => Implementation.AllKeysDown(keys);

    /// <inheritdoc cref="IKeyboard.AllKeysPressed(IEnumerable{Keys})"/>
    public static bool AllKeysPressed(IEnumerable<Keys> keys) => Implementation.AllKeysPressed(keys);

    /// <inheritdoc cref="IKeyboard.AllKeysPressed(Keys[])"/>
    public static bool AllKeysPressed(params Keys[] keys) => Implementation.AllKeysPressed(keys);

    /// <inheritdoc cref="IKeyboard.AllKeysReleased(IEnumerable{Keys})"/>
    public static bool AllKeysReleased(IEnumerable<Keys> keys) => Implementation.AllKeysReleased(keys);

    /// <inheritdoc cref="IKeyboard.AllKeysReleased(Keys[])"/>
    public static bool AllKeysReleased(params Keys[] keys) => Implementation.AllKeysReleased(keys);

    /// <inheritdoc cref="KeyboardExtensions.CombinationDown(IKeyboard, KeyCombination)"/>
    public static bool CombinationDown(KeyCombination combination) => Implementation.CombinationDown(combination);

    /// <inheritdoc cref="KeyboardExtensions.CombinationPressed(IKeyboard, KeyCombination)"/>
    public static bool CombinationPressed(KeyCombination combination) => Implementation.CombinationPressed(combination);
}
