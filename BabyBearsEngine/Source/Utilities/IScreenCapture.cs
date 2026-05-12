namespace BabyBearsEngine;

/// <summary>
/// Test/diagnostic access to the engine's rendered output. When
/// <see cref="DiagnosticsSettings.CaptureFrames"/> is enabled, the engine automatically captures the
/// back buffer at the end of each <see cref="Worlds.IWorld.Draw"/> and exposes it via
/// <see cref="LatestFrame"/>.
/// Coordinates use window space (top-left origin, Y increasing downward), matching the mouse and
/// window APIs.
/// </summary>
public interface IScreenCapture
{
    /// <summary>
    /// The most recently captured frame, indexed <c>[y, x]</c> with row 0 being the top row. The
    /// returned array is the engine's internal storage — do not mutate it. Before the first capture
    /// (or for a 0x0 window), the array is empty.
    /// </summary>
    Colour[,] LatestFrame { get; }

    /// <summary>
    /// Forces an immediate read of the current back buffer, overwriting <see cref="LatestFrame"/>.
    /// Use this only when you need partial-render state — e.g. asserting on a graphic before
    /// something else draws over it. Call from inside <see cref="Worlds.IWorld.Draw"/>; calling
    /// elsewhere yields undefined back-buffer contents.
    /// </summary>
    void CaptureCurrentBackbuffer();

    /// <summary>Reads a single pixel from <see cref="LatestFrame"/>. Shortcut for <c>LatestFrame[y, x]</c>.</summary>
    /// <param name="x">X coordinate in window space (0 is the left edge).</param>
    /// <param name="y">Y coordinate in window space (0 is the top edge).</param>
    Colour GetPixel(int x, int y);
}
