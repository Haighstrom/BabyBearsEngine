using System.Collections.Generic;
using BabyBearsEngine.Runtime;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Input;

public static class Keyboard
{
    private static IKeyboard Implementation => EngineConfiguration.KeyboardService;

    public static bool KeyDown(Keys key) => Implementation.KeyDown(key);
    public static bool KeyPressed(Keys key) => Implementation.KeyPressed(key);
    public static bool KeyReleased(Keys key) => Implementation.KeyReleased(key);

    public static bool AnyKeyDown(IEnumerable<Keys> keys) => Implementation.AnyKeyDown(keys);
    public static bool AnyKeyDown(params Keys[] keys) => Implementation.AnyKeyDown(keys);

    public static bool AnyKeyPressed(IEnumerable<Keys> keys) => Implementation.AnyKeyPressed(keys);
    public static bool AnyKeyPressed(params Keys[] keys) => Implementation.AnyKeyPressed(keys);

    public static bool AnyKeyReleased(IEnumerable<Keys> keys) => Implementation.AnyKeyReleased(keys);
    public static bool AnyKeyReleased(params Keys[] keys) => Implementation.AnyKeyReleased(keys);

    public static bool AllKeysDown(IEnumerable<Keys> keys) => Implementation.AllKeysDown(keys);
    public static bool AllKeysDown(params Keys[] keys) => Implementation.AllKeysDown(keys);

    public static bool AllKeysPressed(IEnumerable<Keys> keys) => Implementation.AllKeysPressed(keys);
    public static bool AllKeysPressed(params Keys[] keys) => Implementation.AllKeysPressed(keys);

    public static bool AllKeysReleased(IEnumerable<Keys> keys) => Implementation.AllKeysReleased(keys);
    public static bool AllKeysReleased(params Keys[] keys) => Implementation.AllKeysReleased(keys);
}

