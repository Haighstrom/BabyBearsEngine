using System.Drawing;
using BabyBearsEngine.Source.Runtime;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;

namespace BabyBearsEngine;

public static class Window
{
    private static IWindow Implementation => EngineConfiguration.WindowService;

    public static WindowBorder Border { get => Implementation.Border; set => Implementation.Border = value; }
    public static bool CursorLockedToWindow { get => Implementation.CursorLockedToWindow; set => Implementation.CursorLockedToWindow = value; }
    public static CursorShape Cursor { get => Implementation.Cursor; set => Implementation.Cursor = value; }
    public static bool CursorVisible { get => Implementation.CursorVisible; set => Implementation.CursorVisible = value; }
    public static bool CloseOnXButton { get => Implementation.CloseOnXButton; set => Implementation.CloseOnXButton = value; }
    public static int Height => Implementation.Height;
    public static WindowIcon Icon { get => Implementation.Icon; set => Implementation.Icon = value; }
    public static Point MaxClientSize { get => Implementation.MaxClientSize; set => Implementation.MaxClientSize = value; }
    public static Point MinClientSize { get => Implementation.MinClientSize; set => Implementation.MinClientSize = value; }
    public static WindowState State { get => Implementation.State; set => Implementation.State = value; }
    public static string Title { get => Implementation.Title; set => Implementation.Title = value; }
    public static bool VSync { get => Implementation.VSync; set => Implementation.VSync = value; }
    public static int Width => Implementation.Width;
    public static int X { get => Implementation.X; set => Implementation.X = value; }
    public static int Y { get => Implementation.Y; set => Implementation.Y = value; }

    public static event Action<ResizeEventArgs> Resize
    {
        add => Implementation.Resize += value;
        remove => Implementation.Resize -= value;
    }

    public static void Centre() => Implementation.Centre();
    public static void Close() => Implementation.Close();
}
