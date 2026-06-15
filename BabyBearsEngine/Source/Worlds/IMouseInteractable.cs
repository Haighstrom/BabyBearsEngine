using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>Represents an addable entity that occupies a region on screen and can be tested for mouse interaction.</summary>
public interface IMouseInteractable : IAddable
{
    /// <summary>Returns the screen-space bounds of this entity.</summary>
    Rect PositionOnScreen { get; }

    /// <summary>
    /// The screen-space rect used for mouse hit testing. Defaults to <see cref="PositionOnScreen"/>;
    /// override to define a click area independent of the entity's bounding rect.
    /// </summary>
    /// <remarks>
    /// Overrides must return window-space coordinates (the same space as <see cref="PositionOnScreen"/>),
    /// since hit testing compares against <c>Mouse.ClientX/Y</c>. Use
    /// <c>Parent?.GetWindowCoordinates(localX, localY)</c> to transform a local-space offset to
    /// window space, exactly as <see cref="PositionOnScreen"/> does.
    /// </remarks>
    Rect HitRect => PositionOnScreen;

    /// <summary>
    /// When true, the entity is excluded from mouse interaction — its click controller skips
    /// it entirely, so no click, hover, or scroll events fire. Defaults to false.
    /// </summary>
    bool Disabled => false;
}
