namespace BabyBearsEngine;

/// <summary>
/// The display state of an application window. Mirrors OpenTK's <c>WindowState</c> with the same integer values.
/// </summary>
public enum WindowState
{
    /// <summary>The window is shown at its set size and position.</summary>
    Normal = 0,
    /// <summary>The window is minimised to the taskbar.</summary>
    Minimized = 1,
    /// <summary>The window is maximised but still windowed (respects the taskbar).</summary>
    Maximized = 2,
    /// <summary>The window covers the entire display.</summary>
    Fullscreen = 3,
}
