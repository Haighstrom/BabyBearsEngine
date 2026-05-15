using BabyBearsEngine.Geometry;

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

    /// <summary>True when the mouse cursor is within this camera's screen-space bounds.</summary>
    bool MouseIntersecting { get; }

    /// <summary>MSAA sample count for this camera's render target.</summary>
    MsaaSamples MSAASamples { get; set; }

    /// <summary>The view transform controlling which region of world space this camera displays.</summary>
    Cameras.ICameraView View { get; }

    /// <summary>Raised when the camera's view changes (scroll, zoom, resize).</summary>
    event EventHandler? ViewChanged;
}
