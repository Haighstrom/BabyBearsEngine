namespace BabyBearsEngine;

/// <summary>
/// An icon for a window's title bar and taskbar entry. Holds one or more <see cref="WindowIconImage"/>
/// bitmaps at different resolutions; the OS picks whichever size best matches each context — small
/// for the taskbar / title bar, larger for window headers and large-icon views. Supplying several
/// sizes lets the OS avoid scaling a single bitmap, which keeps the icon crisp everywhere.
/// </summary>
public sealed record WindowIcon
{
    /// <summary>Creates an empty icon (no image data). Use this when a window should have no icon.</summary>
    public WindowIcon() : this([])
    {
    }

    /// <summary>Creates a single-image icon. Convenience overload for the common one-resolution case.</summary>
    /// <param name="width">Icon width in pixels.</param>
    /// <param name="height">Icon height in pixels.</param>
    /// <param name="pixels">RGBA pixel data, row-major, with one byte per channel.</param>
    public WindowIcon(int width, int height, byte[] pixels) : this(new WindowIconImage(width, height, pixels))
    {
    }

    /// <summary>
    /// Creates an icon from one or more resolution variants. Pass several sizes of the same icon
    /// (e.g. 16×16, 32×32, 48×48) and the OS selects the best fit per context.
    /// </summary>
    /// <param name="images">The resolution variants. Pass none for an empty icon.</param>
    public WindowIcon(params WindowIconImage[] images)
    {
        Images = images;
    }

    /// <summary>The icon's resolution variants. Empty when the window should use the platform default icon.</summary>
    public IReadOnlyList<WindowIconImage> Images { get; }

    /// <summary>True when no image data has been supplied — the window will use the platform default icon.</summary>
    public bool IsEmpty => Images.Count == 0;
}
