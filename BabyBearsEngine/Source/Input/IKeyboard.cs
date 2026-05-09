using System.Collections.Generic;

namespace BabyBearsEngine.Input;

/// <summary>
/// Per-frame keyboard input snapshot. Methods report state as of the current frame:
/// <c>...Down</c> means the key is currently held; <c>...Pressed</c> and <c>...Released</c>
/// detect the single frame of a state change (edge-triggered).
/// </summary>
public interface IKeyboard
{
    /// <summary>Returns true if <paramref name="key"/> is currently held down.</summary>
    bool KeyDown(Keys key);

    /// <summary>Returns true on the single frame that <paramref name="key"/> transitioned from up to down.</summary>
    bool KeyPressed(Keys key);

    /// <summary>Returns true on the single frame that <paramref name="key"/> transitioned from down to up.</summary>
    bool KeyReleased(Keys key);

    /// <summary>Returns true if any key in <paramref name="keys"/> is currently held down.</summary>
    bool AnyKeyDown(IEnumerable<Keys> keys);

    /// <summary>Returns true if any key in <paramref name="keys"/> is currently held down.</summary>
    bool AnyKeyDown(params Keys[] keys);

    /// <summary>Returns true if any key in <paramref name="keys"/> was pressed this frame.</summary>
    bool AnyKeyPressed(IEnumerable<Keys> keys);

    /// <summary>Returns true if any key in <paramref name="keys"/> was pressed this frame.</summary>
    bool AnyKeyPressed(params Keys[] keys);

    /// <summary>Returns true if any key in <paramref name="keys"/> was released this frame.</summary>
    bool AnyKeyReleased(IEnumerable<Keys> keys);

    /// <summary>Returns true if any key in <paramref name="keys"/> was released this frame.</summary>
    bool AnyKeyReleased(params Keys[] keys);

    /// <summary>Returns true only if every key in <paramref name="keys"/> is currently held down.</summary>
    bool AllKeysDown(IEnumerable<Keys> keys);

    /// <summary>Returns true only if every key in <paramref name="keys"/> is currently held down.</summary>
    bool AllKeysDown(params Keys[] keys);

    /// <summary>Returns true only if every key in <paramref name="keys"/> was pressed this frame.</summary>
    bool AllKeysPressed(IEnumerable<Keys> keys);

    /// <summary>Returns true only if every key in <paramref name="keys"/> was pressed this frame.</summary>
    bool AllKeysPressed(params Keys[] keys);

    /// <summary>Returns true only if every key in <paramref name="keys"/> was released this frame.</summary>
    bool AllKeysReleased(IEnumerable<Keys> keys);

    /// <summary>Returns true only if every key in <paramref name="keys"/> was released this frame.</summary>
    bool AllKeysReleased(params Keys[] keys);
}
