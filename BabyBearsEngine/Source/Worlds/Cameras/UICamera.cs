using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// A camera that renders its children at a fixed 1:1 pixel-to-screen mapping with no
/// tile scaling, scrolling, or world-space transforms. Intended for UI overlays that must
/// stay in screen space regardless of what the game camera is doing.
/// </summary>
public sealed class UICamera : Camera
{
    /// <param name="x">Camera X position in screen space.</param>
    /// <param name="y">Camera Y position in screen space.</param>
    /// <param name="width">Viewport width in pixels.</param>
    /// <param name="height">Viewport height in pixels.</param>
    /// <param name="samples">MSAA sample count for the render target. When omitted, uses <see cref="ApplicationSettings.DefaultCameraMsaa"/>.</param>
    public UICamera(float x, float y, float width, float height, MsaaSamples? samples = null)
        : base(x, y, width, height, (getW, getH) => new FixedTileSizeCameraView(1, 1, getW, getH), samples)
    {
        BackgroundColour = Colour.Transparent;
    }
}
