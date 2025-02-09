using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Input;

public static class Keyboard
{
    private static GameWindow? s_window;
    private static KeyboardState? s_currentState;
    private static KeyboardState? s_previousState;

    internal static void Initialise(GameWindow window)
    {
        s_window = window;
        s_currentState = s_previousState = window.KeyboardState;
    }

    internal static void Update()
    {
        if (s_window is null)
        {
            throw new InvalidOperationException("Keyboard has not been initialised.");
        }

        s_previousState = s_currentState;
        s_currentState = s_window.KeyboardState;
    }

    public static bool KeyDown(Keys key)
    {
        if (s_currentState is null)
        {
            throw new InvalidOperationException("Keyboard has not been initialised.");
        }

        return s_currentState.IsKeyDown(key);
    }

    public static bool KeyPressed(Keys key)
    {
        if (s_currentState is null || s_previousState is null)
        {
            throw new InvalidOperationException("Keyboard has not been initialised.");
        }

        return s_currentState.IsKeyDown(key) && !s_previousState.IsKeyDown(key);
    }

    public static bool KeyReleased(Keys key)
    {
        if (s_currentState is null || s_previousState is null)
        {
            throw new InvalidOperationException("Keyboard has not been initialised.");
        }

        return !s_currentState.IsKeyDown(key) && s_previousState.IsKeyDown(key);
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
