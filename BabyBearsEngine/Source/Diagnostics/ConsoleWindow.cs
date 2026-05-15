using System.Runtime.InteropServices;
using BabyBearsEngine.Platform;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Manages the engine's diagnostic console window. Windows-only — on other platforms the launching
/// terminal is already the process's console and there's nothing to allocate or hide; all methods
/// no-op (with a single warning logged the first time). Internal: game devs configure visibility
/// and position via <see cref="ConsoleSettings"/> at startup rather than calling these directly.
/// </summary>
internal static class ConsoleWindow
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

        NativeMethods.AllocConsole();
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

        NativeMethods.FreeConsole();
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

        IntPtr handle = NativeMethods.GetConsoleWindow();
        if (handle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.MoveWindow(handle, x, y, width, height, bRepaint: true);
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

        IntPtr handle = NativeMethods.GetConsoleWindow();
        if (handle == IntPtr.Zero)
        {
            return;
        }

        IntPtr insertAfter = topmost ? NativeMethods.HWND_TOPMOST : NativeMethods.HWND_NOTOPMOST;
        NativeMethods.SetWindowPos(handle, insertAfter, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
    }

    private static (int Width, int Height) GetMaxWorkArea()
    {
        if (!OperatingSystem.IsWindows())
        {
            return (0, 0);
        }

        IntPtr hWnd = NativeMethods.GetConsoleWindow();
        IntPtr monitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MONITOR_DEFAULTTOPRIMARY);
        NativeMethods.MONITORINFO info = new() { Size = (uint)Marshal.SizeOf<NativeMethods.MONITORINFO>() };

        if (!NativeMethods.GetMonitorInfo(monitor, ref info))
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
}
