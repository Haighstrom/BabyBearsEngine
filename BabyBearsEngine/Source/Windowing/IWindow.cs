using System.Drawing;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;

namespace BabyBearsEngine;

public interface IWindow
{
    WindowBorder Border { get; set; }
    bool CursorLockedToWindow { get; set; }
    CursorShape Cursor { get; set; }
    bool CursorVisible { get; set; }
    bool ExitOnClose { get; set; }
    int Height { get; }
    WindowIcon Icon { get; set; }
    Point MaxClientSize { get; set; }
    Point MinClientSize { get; set; }
    WindowState State { get; set; }
    string Title { get; set; }
    bool VSync { get; set; }
    int Width { get; }
    int X { get; set; }
    int Y { get; set; }

    event Action<ResizeEventArgs>? Resize;

    void Centre();
}
