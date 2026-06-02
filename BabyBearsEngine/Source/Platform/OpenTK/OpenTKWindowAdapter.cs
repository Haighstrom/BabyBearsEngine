using System.Drawing;
using OpenTK.Mathematics;

using OpenTKCursorState = OpenTK.Windowing.Common.CursorState;
using OpenTKResizeEventArgs = OpenTK.Windowing.Common.ResizeEventArgs;
using OpenTKVSyncMode = OpenTK.Windowing.Common.VSyncMode;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKWindowAdapter : IWindow
{
    private readonly OpenTKGameEngine _engine;
    private CursorShape _cursor;

    public OpenTKWindowAdapter(OpenTKGameEngine engine, CursorShape initialCursor)
    {
        _engine = engine;
        // Seed the cursor cache from the configured initial cursor. The engine sets the same
        // value on the underlying window in OnLoad — without this seed, the adapter's cached
        // value would forever report Default until something writes through Cursor's setter.
        _cursor = initialCursor;
        _engine.Resize += OnEngineResize;
    }

    public WindowBorder Border
    {
        get => _engine.WindowBorder.ToWindowBorder();
        set => _engine.WindowBorder = value.ToOpenTK();
    }

    public bool CursorLockedToWindow
    {
        get => _engine.CursorState == OpenTKCursorState.Grabbed;
        set => _engine.CursorState = value ? OpenTKCursorState.Grabbed : OpenTKCursorState.Normal;
    }

    public CursorShape Cursor
    {
        get => _cursor;
        set
        {
            _cursor = value;
            _engine.Cursor = value.ToOpenTK();
        }
    }

    public bool CursorVisible
    {
        get => _engine.CursorState == OpenTKCursorState.Normal;
        set
        {
            if (_engine.CursorState != OpenTKCursorState.Grabbed)
            {
                _engine.CursorState = value ? OpenTKCursorState.Normal : OpenTKCursorState.Hidden;
            }
        }
    }

    public bool CloseOnXButton
    {
        get => _engine.CloseOnXButton;
        set => _engine.CloseOnXButton = value;
    }

    public int Height => _engine.ClientSize.Y;

    public WindowIcon Icon
    {
        get => _engine.Icon?.ToWindowIcon() ?? new WindowIcon();
        set => _engine.Icon = value.ToOpenTK();
    }

    public Point MaxClientSize
    {
        get
        {
            var s = _engine.MaximumSize;
            return s.HasValue ? new Point(s.Value.X, s.Value.Y) : new Point();
        }
        set => _engine.MaximumSize = value.IsEmpty ? null : new Vector2i(value.X, value.Y);
    }

    public Point MinClientSize
    {
        get
        {
            var s = _engine.MinimumSize;
            return s.HasValue ? new Point(s.Value.X, s.Value.Y) : new Point();
        }
        set => _engine.MinimumSize = value.IsEmpty ? null : new Vector2i(value.X, value.Y);
    }

    public WindowState State
    {
        get => _engine.WindowState.ToWindowState();
        set => _engine.WindowState = value.ToOpenTK();
    }

    public string Title
    {
        get => _engine.Title;
        set => _engine.Title = value;
    }

    public bool VSync
    {
        get => _engine.VSync == OpenTKVSyncMode.On;
        set => _engine.VSync = value ? OpenTKVSyncMode.On : OpenTKVSyncMode.Off;
    }

    public int Width => _engine.ClientSize.X;

    public int X
    {
        get => _engine.Location.X;
        set => _engine.Location = new Vector2i(value, _engine.Location.Y);
    }

    public int Y
    {
        get => _engine.Location.Y;
        set => _engine.Location = new Vector2i(_engine.Location.X, value);
    }

    public event Action<WindowResizeEventArgs>? Resize;

    public void Centre() => _engine.CenterWindow();

    public void Close()
    {
        _engine._programmaticClose = true;
        _engine.Close();
    }

    private void OnEngineResize(OpenTKResizeEventArgs e)
    {
        Resize?.Invoke(new WindowResizeEventArgs(e.Width, e.Height));
    }
}
