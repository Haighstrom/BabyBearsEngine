using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// A camera that renders a region of world space onto a viewport on screen.
/// </summary>
public interface ICamera : IRect, IUpdateable, IRenderable, IContainer
{
    /// <summary>The colour used to clear the camera's background each frame.</summary>
    Colour BackgroundColour { get; set; }

    /// <summary>Time-scale multiplier applied to all updates within this camera. 1 = normal speed.</summary>
    float GameSpeed { get; set; }

    /// <summary>Maximum X world-space coordinate of the world this camera observes.</summary>
    float MaxX { get; set; }

    /// <summary>Maximum Y world-space coordinate of the world this camera observes.</summary>
    float MaxY { get; set; }

    /// <summary>Minimum X world-space coordinate of the world this camera observes.</summary>
    float MinX { get; set; }

    /// <summary>Minimum Y world-space coordinate of the world this camera observes.</summary>
    float MinY { get; set; }

    /// <summary>True when the mouse cursor is within this camera's screen-space bounds.</summary>
    bool MouseIntersecting { get; }

    /// <summary>
    /// The cursor's current position in this camera's world-space coordinates. Inverts the
    /// camera's window transform (parent chain + view scroll/zoom) so callers don't have to.
    /// </summary>
    Point MouseWorldPosition
    {
        get
        {
            var (winOriginX, winOriginY) = GetWindowCoordinates(0f, 0f);
            float worldX = (Mouse.ClientX - winOriginX) / View.TileWidth;
            float worldY = (Mouse.ClientY - winOriginY) / View.TileHeight;
            return new Point(worldX, worldY);
        }
    }

    /// <summary>MSAA sample count for this camera's render target.</summary>
    MsaaSamples MSAASamples { get; set; }

    /// <summary>The view transform controlling which region of world space this camera displays.</summary>
    Cameras.ICameraView View { get; }

    /// <summary>Raised when the camera's view changes (scroll, zoom, resize).</summary>
    event EventHandler? ViewChanged;
}
