using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Input;

public static class Keyboard
{
    private static KeyboardState? s_keyboardState;

    internal static void Initialise(KeyboardState keyboardState)
    {
        s_keyboardState = keyboardState;
    }

    public static bool KeyDown(Keys key)
    {
        Ensure.NotNull(s_keyboardState);

        return s_keyboardState.IsKeyDown(key);
    }

    public static bool KeyPressed(Keys key)
    {
        Ensure.NotNull(s_keyboardState);

        return s_keyboardState.IsKeyPressed(key);
    }

    public static bool KeyReleased(Keys key)
    {
        Ensure.NotNull(s_keyboardState);

        return s_keyboardState.IsKeyReleased(key);
    }

    public static bool AnyKeyDown(IEnumerable<Keys> keys) => keys.Any(KeyDown);

    public static bool AnyKeyDown(params Keys[] keys) => keys.Any(KeyDown);

    public static bool AnyKeyPressed(IEnumerable<Keys> keys) => keys.Any(KeyPressed);

    public static bool AnyKeyPressed(params Keys[] keys) => keys.Any(KeyPressed);

    public static bool AnyKeyReleased(IEnumerable<Keys> keys) => keys.Any(KeyReleased);

    public static bool AnyKeyReleased(params Keys[] keys) => keys.Any(KeyReleased);

    public static bool AllKeysDown(IEnumerable<Keys> keys) => keys.All(KeyDown);

    public static bool AllKeysDown(params Keys[] keys) => keys.All(KeyDown);

    public static bool AllKeysPressed(IEnumerable<Keys> keys) => keys.All(KeyPressed);

    public static bool AllKeysPressed(params Keys[] keys) => keys.All(KeyPressed);

    public static bool AllKeysReleased(IEnumerable<Keys> keys) => keys.All(KeyReleased);

    public static bool AllKeysReleased(params Keys[] keys) => keys.All(KeyReleased);
}
