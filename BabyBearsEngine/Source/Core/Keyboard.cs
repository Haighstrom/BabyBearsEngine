using System.Collections.Generic;
using BabyBearsEngine.Source.Services;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Core;

internal static class Keyboard
{
    private static IKeyboardService Service => GameServices.KeyboardService;

    public static bool KeyDown(Keys key) => Service.KeyDown(key);
    public static bool KeyPressed(Keys key) => Service.KeyPressed(key);
    public static bool KeyReleased(Keys key) => Service.KeyReleased(key);

    public static bool AnyKeyDown(IEnumerable<Keys> keys) => Service.AnyKeyDown(keys);
    public static bool AnyKeyDown(params Keys[] keys) => Service.AnyKeyDown(keys);

    public static bool AnyKeyPressed(IEnumerable<Keys> keys) => Service.AnyKeyPressed(keys);
    public static bool AnyKeyPressed(params Keys[] keys) => Service.AnyKeyPressed(keys);

    public static bool AnyKeyReleased(IEnumerable<Keys> keys) => Service.AnyKeyReleased(keys);
    public static bool AnyKeyReleased(params Keys[] keys) => Service.AnyKeyReleased(keys);

    public static bool AllKeysDown(IEnumerable<Keys> keys) => Service.AllKeysDown(keys);
    public static bool AllKeysDown(params Keys[] keys) => Service.AllKeysDown(keys);

    public static bool AllKeysPressed(IEnumerable<Keys> keys) => Service.AllKeysPressed(keys);
    public static bool AllKeysPressed(params Keys[] keys) => Service.AllKeysPressed(keys);

    public static bool AllKeysReleased(IEnumerable<Keys> keys) => Service.AllKeysReleased(keys);
    public static bool AllKeysReleased(params Keys[] keys) => Service.AllKeysReleased(keys);
}

