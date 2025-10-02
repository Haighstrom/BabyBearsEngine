using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BabyBearsEngine.Source.Services;

public interface IKeyboardService
{
    bool KeyDown(Keys key);
    bool KeyPressed(Keys key);
    bool KeyReleased(Keys key);

    bool AnyKeyDown(IEnumerable<Keys> keys);
    bool AnyKeyDown(params Keys[] keys);

    bool AnyKeyPressed(IEnumerable<Keys> keys);
    bool AnyKeyPressed(params Keys[] keys);

    bool AnyKeyReleased(IEnumerable<Keys> keys);
    bool AnyKeyReleased(params Keys[] keys);

    bool AllKeysDown(IEnumerable<Keys> keys);
    bool AllKeysDown(params Keys[] keys);

    bool AllKeysPressed(IEnumerable<Keys> keys);
    bool AllKeysPressed(params Keys[] keys);

    bool AllKeysReleased(IEnumerable<Keys> keys);
    bool AllKeysReleased(params Keys[] keys);
}
