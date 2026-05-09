namespace BabyBearsEngine;

/// <summary>
/// The border style of an application window. Mirrors OpenTK's <c>WindowBorder</c> with the same integer values.
/// </summary>
public enum WindowBorder
{
    /// <summary>The window has a sizing border the user can drag to resize it.</summary>
    Resizable = 0,
    /// <summary>The window has a non-resizable border.</summary>
    Fixed = 1,
    /// <summary>The window has no border at all.</summary>
    Hidden = 2,
}
