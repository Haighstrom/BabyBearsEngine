using BabyBearsEngine.Input;

using OpenTKKeyboardState = OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState;

namespace BabyBearsEngine.Platform.OpenTK;

internal class OpenTKKeyboardAdapter(OpenTKKeyboardState keyboardState) : IKeyboard
{
    public bool KeyDown(Keys key) => keyboardState.IsKeyDown(key.ToOpenTK());
    public bool KeyPressed(Keys key) => keyboardState.IsKeyPressed(key.ToOpenTK());
    public bool KeyReleased(Keys key) => keyboardState.IsKeyReleased(key.ToOpenTK());

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
