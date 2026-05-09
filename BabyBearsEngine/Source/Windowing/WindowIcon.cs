namespace BabyBearsEngine;

/// <summary>
/// An icon image used for a window's title bar and taskbar entry. Pixels are RGBA bytes in
/// row-major order, so <see cref="Pixels"/> length must equal <see cref="Width"/> × <see cref="Height"/> × 4.
/// </summary>
/// <param name="width">Icon width in pixels.</param>
/// <param name="height">Icon height in pixels.</param>
/// <param name="pixels">RGBA pixel data, row-major, with one byte per channel.</param>
public sealed class WindowIcon(int width, int height, byte[] pixels)
{
    /// <summary>Creates an empty icon (no image data). Use this when a window should have no icon.</summary>
    public WindowIcon() : this(0, 0, []) { }

    /// <summary>Icon width in pixels.</summary>
    public int Width { get; } = width;

    /// <summary>Icon height in pixels.</summary>
    public int Height { get; } = height;

    /// <summary>True when no pixel data has been supplied — the window will use the platform default icon.</summary>
    public bool IsEmpty => Pixels.Length == 0;

    /// <summary>RGBA pixel data, row-major, with one byte per channel.</summary>
    public byte[] Pixels { get; } = pixels;
}
