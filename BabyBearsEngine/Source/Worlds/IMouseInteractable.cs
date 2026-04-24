using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>Represents an entity that occupies a region on screen and can be tested for mouse interaction.</summary>
public interface IMouseInteractable
{
    /// <summary>Returns the screen-space bounds of this entity.</summary>
    Rect PositionOnScreen { get; }
}
