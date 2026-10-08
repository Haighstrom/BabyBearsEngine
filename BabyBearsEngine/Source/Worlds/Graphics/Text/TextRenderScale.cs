using System.Diagnostics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Tracks how many framebuffer pixels each canvas unit covers when a fixed canvas is stretched to the
/// window, so <see cref="TextGraphic"/> can rasterise glyphs at the size they are really displayed at
/// instead of magnifying (or minifying) a bitmap authored for the canvas size. Layout and measuring
/// stay in canvas units; only the glyph images change. The scale is 1:1 (inactive) when the canvas
/// follows the window.
/// </summary>
internal static class TextRenderScale
{
    private const float MaxScale = 4f;
    private const float MinScale = 0.5f;
    private const float InactiveTolerance = 0.001f;

    // A resize drag changes the ratio every few pixels; waiting for it to hold still avoids
    // rasterising a fresh atlas for every font at every intermediate size.
    private const long SettleMilliseconds = 150;

    private static int s_framebufferHeight = 0;
    private static int s_framebufferWidth = 0;
    private static bool s_hasPending = false;
    private static float s_pendingX = 1f;
    private static float s_pendingY = 1f;
    private static long s_pendingSinceTimestamp = 0;
    private static bool s_subscribed = false;

    /// <summary>Increments whenever the applied scale changes, so graphics know to rebuild their glyph quads.</summary>
    internal static int Version { get; private set; } = 0;

    /// <summary>True when glyphs should be rasterised at a size other than the canvas size.</summary>
    internal static bool IsActive => MathF.Abs(X - 1f) > InactiveTolerance || MathF.Abs(Y - 1f) > InactiveTolerance;

    /// <summary>Framebuffer pixels per canvas unit, horizontally.</summary>
    internal static float X { get; private set; } = 1f;

    /// <summary>Framebuffer pixels per canvas unit, vertically.</summary>
    internal static float Y { get; private set; } = 1f;

    /// <summary>The scale glyph atlases are rasterised at: the larger axis, so neither axis is under-resolved.</summary>
    internal static float AtlasScale => MathF.Max(X, Y);

    /// <summary>Picks up the current window and canvas sizes, applying a changed scale once the window has stopped resizing.</summary>
    internal static void Refresh()
    {
        if (!Canvas.HasFixedSize)
        {
            if (IsActive)
            {
                Apply(1f, 1f);
            }

            return;
        }

        EnsureSubscribed();

        int framebufferWidth = s_framebufferWidth > 0 ? s_framebufferWidth : Window.Width;
        int framebufferHeight = s_framebufferHeight > 0 ? s_framebufferHeight : Window.Height;

        if (framebufferWidth <= 0 || framebufferHeight <= 0)
        {
            return;
        }

        (float targetX, float targetY) = Compute(framebufferWidth, framebufferHeight, Canvas.Width, Canvas.Height);

        if (targetX == X && targetY == Y)
        {
            s_hasPending = false;
            return;
        }

        // The first real measurement applies straight away; later changes wait for the window to settle.
        if (Version == 0)
        {
            Apply(targetX, targetY);
            return;
        }

        long now = Stopwatch.GetTimestamp();

        if (!s_hasPending || targetX != s_pendingX || targetY != s_pendingY)
        {
            s_hasPending = true;
            s_pendingX = targetX;
            s_pendingY = targetY;
            s_pendingSinceTimestamp = now;
            return;
        }

        if (Stopwatch.GetElapsedTime(s_pendingSinceTimestamp).TotalMilliseconds >= SettleMilliseconds)
        {
            Apply(targetX, targetY);
        }
    }

    /// <summary>Forgets the tracked sizes and returns to 1:1; used when the engine is reset.</summary>
    internal static void Reset()
    {
        s_framebufferHeight = 0;
        s_framebufferWidth = 0;
        s_hasPending = false;
        s_subscribed = false;
        X = 1f;
        Y = 1f;
        Version = 0;
    }

    /// <summary>Framebuffer pixels per canvas unit on each axis, clamped to a sane range.</summary>
    internal static (float X, float Y) Compute(int framebufferWidth, int framebufferHeight, int canvasWidth, int canvasHeight)
        => (Math.Clamp(framebufferWidth / (float)canvasWidth, MinScale, MaxScale),
            Math.Clamp(framebufferHeight / (float)canvasHeight, MinScale, MaxScale));

    private static void Apply(float x, float y)
    {
        X = x;
        Y = y;
        s_hasPending = false;
        Version++;
        FontTextureCache.InvalidateScaled();
    }

    private static void EnsureSubscribed()
    {
        if (s_subscribed)
        {
            return;
        }

        s_subscribed = true;
        Window.FramebufferResize += args =>
        {
            s_framebufferWidth = args.Width;
            s_framebufferHeight = args.Height;
        };
    }
}
