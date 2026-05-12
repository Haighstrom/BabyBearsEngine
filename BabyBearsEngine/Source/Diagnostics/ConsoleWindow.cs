using System.Runtime.InteropServices;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Manages the engine's diagnostic console window. Windows-only — on other platforms the launching
/// terminal is already the process's console and there's nothing to allocate or hide; all methods
/// no-op (with a single warning logged the first time). Internal: game devs configure visibility
/// and position via <see cref="ConsoleSettings"/> at startup rather than calling these directly.
/// </summary>
internal static partial class ConsoleWindow
{
    private static bool s_visible = false;
    private static bool s_unsupportedWarned = false;

    /// <summary>True on Windows where the P/Invoke calls are valid; false elsewhere.</summary>
    public static bool IsSupported => OperatingSystem.IsWindows();

    /// <summary>Whether the engine has a console window allocated. Tracked locally — doesn't detect external closes (e.g. the user clicking the X).</summary>
    public static bool IsVisible => s_visible;

    /// <summary>Work-area width of the monitor the console is on, or of the primary monitor if no console is open. Returns 0 on unsupported platforms or query failure.</summary>
    public static int MaxWidth
    {
        get
        {
            if (!OperatingSystem.IsWindows())
            {
                return 0;
            }
            return GetMaxWorkArea().Width;
        }
    }

    /// <summary>Work-area height of the monitor the console is on, or of the primary monitor if no console is open. Returns 0 on unsupported platforms or query failure.</summary>
    public static int MaxHeight
    {
        get
        {
            if (!OperatingSystem.IsWindows())
            {
                return 0;
            }
            return GetMaxWorkArea().Height;
        }
    }

    /// <summary>Allocates and shows a console for the current process. No-op if already visible or on non-Windows.</summary>
    public static void Open()
    {
        if (!OperatingSystem.IsWindows())
        {
            WarnIfUnsupportedOnce();
            return;
        }

        if (s_visible)
        {
            Logger.Warning("ConsoleWindow.Open called but the console is already open.");
            return;
        }

        AllocConsole();
        s_visible = true;
    }

    /// <summary>Allocates a console and positions it. Convenience wrapper around <see cref="Open()"/> + <see cref="MoveTo"/>.</summary>
    public static void Open(int x, int y, int width, int height)
    {
        Open();

        if (s_visible)
        {
            MoveTo(x, y, width, height);
        }
    }

    /// <summary>Detaches and closes the console. No-op if not visible or on non-Windows.</summary>
    public static void Close()
    {
        if (!OperatingSystem.IsWindows())
        {
            WarnIfUnsupportedOnce();
            return;
        }

        if (!s_visible)
        {
            Logger.Warning("ConsoleWindow.Close called but the console is not currently open.");
            return;
        }

        FreeConsole();
        s_visible = false;
    }

    /// <summary>Repositions and resizes the console window. No-op if no console is allocated or on non-Windows.</summary>
    public static void MoveTo(int x, int y, int width, int height)
    {
        if (!OperatingSystem.IsWindows())
        {
            WarnIfUnsupportedOnce();
            return;
        }

        IntPtr handle = GetConsoleWindow();
        if (handle == IntPtr.Zero)
        {
            return;
        }

        MoveWindow(handle, x, y, width, height, bRepaint: true);
    }

    /// <summary>Sets the console window title. No-op if no console is allocated or on non-Windows.</summary>
    public static void SetTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        if (!OperatingSystem.IsWindows())
        {
            WarnIfUnsupportedOnce();
            return;
        }

        if (!s_visible)
        {
            return;
        }

        Console.Title = title;
    }

    /// <summary>Toggles whether the console window stays above other windows. Useful when debugging a fullscreen game. No-op if no console is allocated or on non-Windows.</summary>
    public static void SetTopmost(bool topmost)
    {
        if (!OperatingSystem.IsWindows())
        {
            WarnIfUnsupportedOnce();
            return;
        }

        IntPtr handle = GetConsoleWindow();
        if (handle == IntPtr.Zero)
        {
            return;
        }

        IntPtr insertAfter = topmost ? HWND_TOPMOST : HWND_NOTOPMOST;
        SetWindowPos(handle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }

    private static (int Width, int Height) GetMaxWorkArea()
    {
        if (!OperatingSystem.IsWindows())
        {
            return (0, 0);
        }

        IntPtr hWnd = GetConsoleWindow();
        IntPtr monitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);
        var info = new MONITORINFO { Size = (uint)Marshal.SizeOf<MONITORINFO>() };

        if (!GetMonitorInfo(monitor, ref info))
        {
            return (0, 0);
        }

        return (info.WorkArea.Right - info.WorkArea.Left, info.WorkArea.Bottom - info.WorkArea.Top);
    }

    private static void WarnIfUnsupportedOnce()
    {
        if (s_unsupportedWarned)
        {
            return;
        }

        Logger.Warning("ConsoleWindow operations are only supported on Windows. Skipping.");
        s_unsupportedWarned = true;
    }

    // ─── P/Invoke imports ───

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FreeConsole();

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

    [LibraryImport("User32.dll", SetLastError = true)]
    private static partial IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

    [LibraryImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [LibraryImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public uint Size;
        public RECT Monitor;
        public RECT WorkArea;
        public uint Flags;
    }
}
