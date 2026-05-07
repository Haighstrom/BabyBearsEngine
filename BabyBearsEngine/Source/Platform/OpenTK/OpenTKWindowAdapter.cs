using System.Drawing;
using OpenTK.Mathematics;

using OpenTKCursorState = OpenTK.Windowing.Common.CursorState;
using OpenTKResizeEventArgs = OpenTK.Windowing.Common.ResizeEventArgs;
using OpenTKVSyncMode = OpenTK.Windowing.Common.VSyncMode;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKWindowAdapter(OpenTKGameEngine engine) : IWindow
{
    private CursorShape _cursor = CursorShape.Default;

    public WindowBorder Border
    {
        get => engine.WindowBorder.ToBBE();
        set => engine.WindowBorder = value.ToOpenTK();
    }

    public bool CursorLockedToWindow
    {
        get => engine.CursorState == OpenTKCursorState.Grabbed;
        set => engine.CursorState = value ? OpenTKCursorState.Grabbed : OpenTKCursorState.Normal;
    }

    public CursorShape Cursor
    {
        get => _cursor;
        set
        {
            _cursor = value;
            engine.Cursor = value.ToOpenTK();
        }
    }

    public bool CursorVisible
    {
        get => engine.CursorState == OpenTKCursorState.Normal;
        set
        {
            if (engine.CursorState != OpenTKCursorState.Grabbed)
            {
                engine.CursorState = value ? OpenTKCursorState.Normal : OpenTKCursorState.Hidden;
            }
        }
    }

    public bool CloseOnXButton
    {
        get => engine.CloseOnXButton;
        set => engine.CloseOnXButton = value;
    }

    public int Height => engine.ClientSize.Y;

    public WindowIcon Icon
    {
        get => engine.Icon?.ToBBE() ?? new WindowIcon();
        set => engine.Icon = value.ToOpenTK();
    }

    public Point MaxClientSize
    {
        get
        {
            var s = engine.MaximumSize;
            return s.HasValue ? new Point(s.Value.X, s.Value.Y) : new Point();
        }
        set => engine.MaximumSize = value.IsEmpty ? null : new Vector2i(value.X, value.Y);
    }

    public Point MinClientSize
    {
        get
        {
            var s = engine.MinimumSize;
            return s.HasValue ? new Point(s.Value.X, s.Value.Y) : new Point();
        }
        set => engine.MinimumSize = value.IsEmpty ? null : new Vector2i(value.X, value.Y);
    }

    public WindowState State
    {
        get => engine.WindowState.ToBBE();
        set => engine.WindowState = value.ToOpenTK();
    }

    public string Title
    {
        get => engine.Title;
        set => engine.Title = value;
    }

    public bool VSync
    {
        get => engine.VSync == OpenTKVSyncMode.On;
        set => engine.VSync = value ? OpenTKVSyncMode.On : OpenTKVSyncMode.Off;
    }

    public int Width => engine.ClientSize.X;

    public int X
    {
        get => engine.Location.X;
        set => engine.Location = new Vector2i(value, engine.Location.Y);
    }

    public int Y
    {
        get => engine.Location.Y;
        set => engine.Location = new Vector2i(engine.Location.X, value);
    }

    public event Action<OpenTKResizeEventArgs>? Resize
    {
        add { engine.Resize += value; }
        remove { engine.Resize -= value; }
    }

    public void Centre() => engine.CenterWindow();

    public void Close()
    {
        engine._programmaticClose = true;
        engine.Close();
    }
}
