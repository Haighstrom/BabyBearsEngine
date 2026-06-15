using System.Drawing;

namespace BabyBearsEngine;

/// <summary>
/// Static facade over the installed <see cref="IWindow"/> service. All members route through
/// <c>EngineConfiguration.WindowService</c>; tests substitute a fake there to exercise consumers
/// without a real window. Throws <see cref="InvalidOperationException"/> if accessed before the engine is initialised.
/// </summary>
/// <remarks>
/// The engine supports a single window per process: there is one shared window, and running two
/// games concurrently in the same process is not supported.
/// </remarks>
public static class Window
{
    private static IWindow Implementation => EngineConfiguration.WindowService;

    /// <inheritdoc cref="IWindow.Border"/>
    public static WindowBorder Border { get => Implementation.Border; set => Implementation.Border = value; }

    /// <inheritdoc cref="IWindow.CursorLockedToWindow"/>
    public static bool CursorLockedToWindow { get => Implementation.CursorLockedToWindow; set => Implementation.CursorLockedToWindow = value; }

    /// <inheritdoc cref="IWindow.Cursor"/>
    public static CursorShape Cursor { get => Implementation.Cursor; set => Implementation.Cursor = value; }

    /// <inheritdoc cref="IWindow.CursorVisible"/>
    public static bool CursorVisible { get => Implementation.CursorVisible; set => Implementation.CursorVisible = value; }

    /// <inheritdoc cref="IWindow.CloseOnXButton"/>
    public static bool CloseOnXButton { get => Implementation.CloseOnXButton; set => Implementation.CloseOnXButton = value; }

    /// <inheritdoc cref="IWindow.Height"/>
    public static int Height => Implementation.Height;

    /// <inheritdoc cref="IWindow.Icon"/>
    public static WindowIcon Icon { get => Implementation.Icon; set => Implementation.Icon = value; }

    /// <inheritdoc cref="IWindow.MaxClientSize"/>
    public static Point MaxClientSize { get => Implementation.MaxClientSize; set => Implementation.MaxClientSize = value; }

    /// <inheritdoc cref="IWindow.MinClientSize"/>
    public static Point MinClientSize { get => Implementation.MinClientSize; set => Implementation.MinClientSize = value; }

    /// <inheritdoc cref="IWindow.State"/>
    public static WindowState State { get => Implementation.State; set => Implementation.State = value; }

    /// <inheritdoc cref="IWindow.Title"/>
    public static string Title { get => Implementation.Title; set => Implementation.Title = value; }

    /// <inheritdoc cref="IWindow.VSync"/>
    public static bool VSync { get => Implementation.VSync; set => Implementation.VSync = value; }

    /// <inheritdoc cref="IWindow.Width"/>
    public static int Width => Implementation.Width;

    /// <inheritdoc cref="IWindow.X"/>
    public static int X { get => Implementation.X; set => Implementation.X = value; }

    /// <inheritdoc cref="IWindow.Y"/>
    public static int Y { get => Implementation.Y; set => Implementation.Y = value; }

    /// <inheritdoc cref="IWindow.Resize"/>
    public static event Action<WindowResizeEventArgs> Resize
    {
        add => Implementation.Resize += value;
        remove => Implementation.Resize -= value;
    }

    /// <inheritdoc cref="IWindow.Centre"/>
    public static void Centre() => Implementation.Centre();

    /// <inheritdoc cref="IWindow.Close"/>
    public static void Close() => Implementation.Close();
}
