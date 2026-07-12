namespace BabyBearsEngine;

/// <summary>
/// The logical rendering canvas — the coordinate space that worlds render in and that
/// <see cref="Input.Mouse"/> reports positions in.
/// <para>
/// When a fixed canvas size is configured (via <see cref="WindowSettings.CanvasWidth"/> /
/// <see cref="WindowSettings.CanvasHeight"/>), every world renders in that coordinate space and
/// is stretched to fill the window, so the game can be laid out against one design resolution
/// and remain resizable. Mouse coordinates are scaled into the same space, so hit-testing is
/// unaffected by the window's actual size.
/// </para>
/// <para>
/// When no fixed size is configured (the default), the canvas follows the window client size and
/// canvas coordinates are window pixels — the engine's historical behaviour.
/// </para>
/// </summary>
public static class Canvas
{
    private static int? s_fixedHeight = null;
    private static int? s_fixedWidth = null;

    /// <summary>True when a fixed canvas size was configured; false when the canvas follows the window size.</summary>
    public static bool HasFixedSize => s_fixedWidth is not null;

    /// <summary>The canvas height in logical units — the fixed height when configured, otherwise the window client height.</summary>
    public static int Height => s_fixedHeight ?? Window.Height;

    /// <summary>The canvas width in logical units — the fixed width when configured, otherwise the window client width.</summary>
    public static int Width => s_fixedWidth ?? Window.Width;

    /// <summary>
    /// Installs the fixed canvas size (both dimensions), or clears it (both null) to follow the
    /// window size. Called by <see cref="GameLauncher"/> from <see cref="WindowSettings"/>.
    /// </summary>
    /// <exception cref="ArgumentException">One dimension is set without the other.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is zero or negative.</exception>
    internal static void Configure(int? width, int? height)
    {
        if (width is null != height is null)
        {
            throw new ArgumentException($"{nameof(WindowSettings.CanvasWidth)} and {nameof(WindowSettings.CanvasHeight)} must be set together (or both left null).");
        }

        if (width is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Canvas width must be positive.");
        }

        if (height is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Canvas height must be positive.");
        }

        s_fixedWidth = width;
        s_fixedHeight = height;
    }

    internal static void Reset()
    {
        s_fixedWidth = null;
        s_fixedHeight = null;
    }
}
