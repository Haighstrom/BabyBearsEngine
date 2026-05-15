namespace BabyBearsEngine.Worlds.Cameras;

/// <summary>
/// Defines the view transform for a camera: scroll position, tile/pixel scale,
/// and coordinate conversion from world space to camera-local space.
/// </summary>
public interface ICameraView
{
    /// <summary>Height of one world tile in pixels.</summary>
    float TileHeight { get; set; }

    /// <summary>Width of one world tile in pixels.</summary>
    float TileWidth { get; set; }

    /// <summary>World-space X origin of the view (left edge of what the camera sees).</summary>
    float X { get; set; }

    /// <summary>World-space Y origin of the view (top edge of what the camera sees).</summary>
    float Y { get; set; }

    /// <summary>Raised when any property of the view changes (scroll, zoom, resize).</summary>
    event EventHandler? ViewChanged;

    /// <summary>Converts a world-space point to camera-local pixel coordinates.</summary>
    (float x, float y) WorldToLocal(float worldX, float worldY);
}
