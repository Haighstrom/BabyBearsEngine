using System.Drawing;

namespace BabyBearsEngine;

/// <summary>
/// The application window. Properties cover its visible state (size, position, title, icon, cursor)
/// and the <see cref="Resize"/> event fires when the client area changes size.
/// </summary>
public interface IWindow
{
    /// <summary>Border style — fixed, resizable, or hidden.</summary>
    WindowBorder Border { get; set; }

    /// <summary>When true, the OS confines the cursor to the window's client area.</summary>
    bool CursorLockedToWindow { get; set; }

    /// <summary>The cursor shape shown while hovering the window.</summary>
    CursorShape Cursor { get; set; }

    /// <summary>When true, the cursor is visible over the window.</summary>
    bool CursorVisible { get; set; }

    /// <summary>When true, clicking the window's close (X) button closes it. When false the click is suppressed and the application must call <see cref="Close"/> to close programmatically.</summary>
    bool CloseOnXButton { get; set; }

    /// <summary>The current client area height in pixels (read-only — set via window resizing or border changes).</summary>
    int Height { get; }

    /// <summary>The window/taskbar icon. Replace with a new <see cref="WindowIcon"/> to change the displayed image.</summary>
    WindowIcon Icon { get; set; }

    /// <summary>Maximum allowed client size. <see cref="Point.IsEmpty"/> means no upper bound.</summary>
    Point MaxClientSize { get; set; }

    /// <summary>Minimum allowed client size. <see cref="Point.IsEmpty"/> means no lower bound.</summary>
    Point MinClientSize { get; set; }

    /// <summary>Window state — normal, minimised, maximised, or fullscreen.</summary>
    WindowState State { get; set; }

    /// <summary>The text shown in the window's title bar.</summary>
    string Title { get; set; }

    /// <summary>When true, the rendering loop syncs to the monitor's refresh rate.</summary>
    bool VSync { get; set; }

    /// <summary>The current client area width in pixels (read-only — set via window resizing or border changes).</summary>
    int Width { get; }

    /// <summary>The window's left edge X coordinate in screen space.</summary>
    int X { get; set; }

    /// <summary>The window's top edge Y coordinate in screen space.</summary>
    int Y { get; set; }

    /// <summary>Fires after the client area has been resized. The event payload carries the new width and height.</summary>
    event Action<WindowResizeEventArgs>? Resize;

    /// <summary>
    /// Fires after the framebuffer has been resized. On Windows and X11 this always fires
    /// alongside <see cref="Resize"/> with identical values, since framebuffer pixels and
    /// client-area pixels map 1:1 on those platforms. On platforms where a window's content
    /// scale can change independently of its screen-coordinate size (macOS, Wayland) — e.g.
    /// dragging the window to a display with a different DPI scale — this can fire without a
    /// corresponding <see cref="Resize"/>. FBO-backed render targets and cameras that need to
    /// track actual pixel resolution should rebuild against this event rather than <see cref="Resize"/>.
    /// </summary>
    event Action<WindowResizeEventArgs>? FramebufferResize;

    /// <summary>Centres the window on the current display.</summary>
    void Centre();

    /// <summary>Closes the window programmatically. Use this when <see cref="CloseOnXButton"/> is false.</summary>
    void Close();
}
