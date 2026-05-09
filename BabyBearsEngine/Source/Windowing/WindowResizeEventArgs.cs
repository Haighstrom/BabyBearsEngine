namespace BabyBearsEngine;

/// <summary>
/// Payload for the <see cref="IWindow.Resize"/> event. Carries the new client-area dimensions
/// after a window resize.
/// </summary>
/// <param name="width">The new client-area width in pixels.</param>
/// <param name="height">The new client-area height in pixels.</param>
public sealed class WindowResizeEventArgs(int width, int height)
{
    /// <summary>The new client-area width in pixels.</summary>
    public int Width { get; } = width;

    /// <summary>The new client-area height in pixels.</summary>
    public int Height { get; } = height;
}
