using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Platform.OpenTK;

internal class OpenTKKeyboardAdapter(KeyboardState keyboardState) : IKeyboard
{
    public bool KeyDown(Keys key)
    {
        return keyboardState.IsKeyDown(key);
    }

    public bool KeyPressed(Keys key)
    {
        return keyboardState.IsKeyPressed(key);
    }

    public bool KeyReleased(Keys key)
    {
        return keyboardState.IsKeyReleased(key);
    }

    public bool AnyKeyDown(IEnumerable<Keys> keys) => keys.Any(KeyDown);

    public bool AnyKeyDown(params Keys[] keys) => keys.Any(KeyDown);

    public bool AnyKeyPressed(IEnumerable<Keys> keys) => keys.Any(KeyPressed);

    public bool AnyKeyPressed(params Keys[] keys) => keys.Any(KeyPressed);

    public bool AnyKeyReleased(IEnumerable<Keys> keys) => keys.Any(KeyReleased);

    public bool AnyKeyReleased(params Keys[] keys) => keys.Any(KeyReleased);

    public bool AllKeysDown(IEnumerable<Keys> keys) => keys.All(KeyDown);

    public bool AllKeysDown(params Keys[] keys) => keys.All(KeyDown);

    public bool AllKeysPressed(IEnumerable<Keys> keys) => keys.All(KeyPressed);

    public bool AllKeysPressed(params Keys[] keys) => keys.All(KeyPressed);

    public bool AllKeysReleased(IEnumerable<Keys> keys) => keys.All(KeyReleased);

    public bool AllKeysReleased(params Keys[] keys) => keys.All(KeyReleased);
}
