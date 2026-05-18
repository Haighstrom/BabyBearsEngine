namespace BabyBearsEngine;

/// <summary>
/// Static facade over the installed <see cref="IScreenCapture"/> service. Mirrors
/// <see cref="Window"/>/<see cref="Mouse"/>/<see cref="Keyboard"/>; tests can substitute the
/// underlying service via <c>EngineConfiguration</c>.
/// <para>
/// Requires <see cref="DiagnosticsSettings.CaptureFrames"/> to be enabled in
/// <see cref="ApplicationSettings"/>; otherwise no capture service is installed and access throws.
/// </para>
/// </summary>
public static class ScreenCapture
{
    private static IScreenCapture Implementation => EngineConfiguration.ScreenCaptureService;

    /// <inheritdoc cref="IScreenCapture.LatestFrame"/>
    public static Colour[,] LatestFrame => Implementation.LatestFrame;

    /// <inheritdoc cref="IScreenCapture.CaptureCurrentBackbuffer"/>
    public static void CaptureCurrentBackbuffer() => Implementation.CaptureCurrentBackbuffer();

    /// <inheritdoc cref="IScreenCapture.GetPixel"/>
    public static Colour GetPixel(int x, int y) => Implementation.GetPixel(x, y);
}
