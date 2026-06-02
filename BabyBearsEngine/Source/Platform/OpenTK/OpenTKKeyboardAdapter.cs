using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;

using OpenTKKeyboardState = OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState;

namespace BabyBearsEngine.Platform.OpenTK;

internal class OpenTKKeyboardAdapter : IKeyboard
{
    private readonly OpenTKGameEngine _engine;
    private OpenTKKeyboardState KeyboardState => _engine.KeyboardState;

    // Keys that were held when the window lost focus and have not yet been physically released
    // since. OpenTK doesn't emit release events while the window is unfocused, so without this
    // suppression set a key held during alt-tab would stay "down" forever. Once the underlying
    // state reports a key as not-down (the user physically released it), it leaves this set
    // and subsequent presses register normally.
    private readonly HashSet<Keys> _suppressedUntilReleased = [];

    public OpenTKKeyboardAdapter(OpenTKGameEngine engine)
    {
        _engine = engine;
        engine.FocusedChanged += OnEngineFocusedChanged;
    }

    private void OnEngineFocusedChanged(FocusedChangedEventArgs e)
    {
        if (!e.IsFocused)
        {
            // Window just lost focus — snapshot held keys so they won't stick, and reset the
            // mouse-over solver so a click-controller from before the alt-tab doesn't keep
            // a stale lock on the next focus-back frame.
            FlushDownKeys();
            MouseSolver.Reset();
        }
    }

    // Keys.Unknown maps to OpenTK -1, which would index a bit array out of range and throw.
    // Short-circuit so callers that legitimately end up with Unknown (e.g. a key with no
    // platform mapping) get a clean "not down" answer instead of an exception.
    public bool KeyDown(Keys key)
    {
        if (key == Keys.Unknown)
        {
            return false;
        }

        bool down = KeyboardState.IsKeyDown(key.ToOpenTK());
        if (_suppressedUntilReleased.Contains(key))
        {
            if (!down)
            {
                _suppressedUntilReleased.Remove(key);
            }
            return false;
        }
        return down;
    }

    public bool KeyPressed(Keys key) => key != Keys.Unknown && !_suppressedUntilReleased.Contains(key) && KeyboardState.IsKeyPressed(key.ToOpenTK());
    public bool KeyReleased(Keys key) => key != Keys.Unknown && KeyboardState.IsKeyReleased(key.ToOpenTK());

    /// <summary>
    /// Marks every key currently reported as down by the underlying state as "needs real
    /// release before counting as down again". Called by <see cref="OpenTKGameEngine"/> when
    /// the window loses focus, since OpenTK doesn't emit release events while unfocused —
    /// without this, a key held during alt-tab would stay stuck down forever.
    /// </summary>
    internal void FlushDownKeys()
    {
        foreach (Keys key in Enum.GetValues<Keys>())
        {
            if (key != Keys.Unknown && KeyboardState.IsKeyDown(key.ToOpenTK()))
            {
                _suppressedUntilReleased.Add(key);
            }
        }
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
