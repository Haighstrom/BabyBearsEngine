using System.Drawing;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKWindowAdapter(OpenTKGameEngine engine) : IWindow
{
    private CursorShape _cursor = CursorShape.Default;

    public WindowBorder Border
    {
        get => engine.WindowBorder;
        set => engine.WindowBorder = value;
    }

    public bool CursorLockedToWindow
    {
        get => engine.CursorState == CursorState.Grabbed;
        set => engine.CursorState = value ? CursorState.Grabbed : CursorState.Normal;
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
        get => engine.CursorState == CursorState.Normal;
        set
        {
            if (engine.CursorState != CursorState.Grabbed)
            {
                engine.CursorState = value ? CursorState.Normal : CursorState.Hidden;
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
        get => engine.Icon ?? new WindowIcon();
        set => engine.Icon = value;
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
        get => engine.WindowState;
        set => engine.WindowState = value;
    }

    public string Title
    {
        get => engine.Title;
        set => engine.Title = value;
    }

    public bool VSync
    {
        get => engine.VSync == VSyncMode.On;
        set => engine.VSync = value ? VSyncMode.On : VSyncMode.Off;
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

    public event Action<ResizeEventArgs>? Resize
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
